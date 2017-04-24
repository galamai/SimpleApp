using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SimpleApp.AzureStorage.Timers;
using SimpleApp.Messaging;
using SimpleApp.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Storage
{
    public class EventStore : IEventStore, IHostTask
    {
        private const string RowKeyVersionUpperLimit = "9999999999";
        private const string UnpublishedRowKeyPrefix = "Unpublished_";
        private const string UnpublishedRowKeyPrefixUpperLimit = "Unpublished`";
        private const string CorellationIdRowKeyPreffix = "Cid_";

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
#if DEBUG
            Formatting = Formatting.Indented
#endif
        };

        private readonly IMessageBus _messageBus;
        private readonly ILogger<EventStore> _logger;
        private readonly EventStoreOptions _options;
        private readonly AsyncUniqueQueue<string> _queue;
        private readonly CloudTable _table;
        private readonly List<Task> _processing;

        public EventStore(
            IMessageBus messageBus,
            ILogger<EventStore> logger,
            IOptions<EventStoreOptions> optionsAccessor)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));

            _queue = new AsyncUniqueQueue<string>();

            var storageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.DefaultRequestOptions = new TableRequestOptions()
            {
                PayloadFormat = TablePayloadFormat.Json
            };
            _table = tableClient.GetTableReference(_options.TableName);

            _processing = new List<Task>();
        }

        public async Task<Slice<IEvent>> LoadAsync(string streamName, int version, CancellationToken cancellationToken)
        {
            var condition = GeneratePartitionKeyWithRowKeySliceFilter(streamName, GetRowKey(version), RowKeyVersionUpperLimit);
            var query = new TableQuery<EventTableEntity>().Where(condition);

            TableQuerySegment<EventTableEntity> tableQueryResult = null;
            try
            {
                tableQueryResult = await _table.ExecuteQuerySegmentedAsync(query, null, null, null, cancellationToken);
            }
            catch(StorageException ex) when (ex.IsNotFoundTableNotFound())
            {
                return new Slice<IEvent>(new List<IEvent>(), false);
            }

            return new Slice<IEvent>(
                tableQueryResult.Select(x => JsonConvert.DeserializeObject<IEvent>(x.Payload, SerializerSettings)),
                tableQueryResult.ContinuationToken != null);
        }

        public Task SaveAsync(string streamName, int expectedVersion, IEnumerable<IEvent> events, string correlationId = null)
        {
            if (streamName == null)
                throw new ArgumentNullException(nameof(streamName));

            if (events == null)
                throw new ArgumentNullException(nameof(events));

            if (events.Count() == 0)
                return Task.CompletedTask;

            return SaveEventsAsync(streamName, expectedVersion, events, correlationId);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using (var timer = TaskSeriesTimer.StartNew(PublishPendingEventsAsync, _logger))
            {
                await Task.WhenAll(
                    EnqueueStreamsWithPendingEventsAsync(cancellationToken),
                    timer.WaitAsync(cancellationToken));
            }
        }

        private async Task SaveEventsAsync(string stream, int expectedVersion, IEnumerable<IEvent> events, string correlationId)
        {
            var batch = new TableBatchOperation();

            if (correlationId != null)
            {
                batch.Add(TableOperation.Insert(new TableEntity(stream, CorellationIdRowKeyPreffix + correlationId)));
            }

            var version = expectedVersion + 1;

            foreach (var evt in events)
            {
                var rowKey = GetRowKey(version);
                var payload = JsonConvert.SerializeObject(evt, SerializerSettings);
                var type = evt.GetType().Name;

                batch.Add(TableOperation.Insert(new EventTableEntity()
                {
                    PartitionKey = stream,
                    RowKey = rowKey,
                    Payload = payload,
                    Type = type,
                    CorrelationId = correlationId
                }));
                batch.Add(TableOperation.Insert(new EventTableEntity()
                {
                    PartitionKey = stream,
                    RowKey = UnpublishedRowKeyPrefix + rowKey,
                    Payload = payload,
                    Type = type,
                    CorrelationId = correlationId
                }));

                version++;
            }

            try
            {
                await SaveAndEnqueueAsync(stream, batch);
            }
            catch (StorageException ex) when (ex.IsConflict())
            {
                var message = ex.RequestInformation.ExtendedErrorInformation.ErrorMessage;

                if (correlationId != null && message.StartsWith("0:"))
                {
                    throw new DuplicateSaveException($"Duplicate commit `{correlationId}` on stream `{stream}`.", ex);
                }

                throw new ConcurrencyException($"Stream `{stream}` already modified.", ex);
            }
        }

        private async Task SaveAndEnqueueAsync(string stream, TableBatchOperation batch)
        {
            try
            {
                await _table.ExecuteBatchAsync(batch);
                await _queue.EnqueueAsync(stream);
            }
            catch (StorageException ex) when (ex.IsNotFoundTableNotFound())
            {
                await _table.CreateIfNotExistsAsync();
                await _table.ExecuteBatchAsync(batch);
                await _queue.EnqueueAsync(stream);
            }
        }

        private async Task EnqueueStreamsWithPendingEventsAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            var condition = GenerateRowKeySliceFilter(UnpublishedRowKeyPrefix, UnpublishedRowKeyPrefixUpperLimit);
            var query = new TableQuery().Where(condition).Select(new string[] { "PartitionKey" });

            try
            {
                TableContinuationToken continuationToken = null;
                do
                {
                    var tableQueryResult = await _table
                        .ExecuteQuerySegmentedAsync(query, continuationToken, null, null, cancellationToken);

                    continuationToken = tableQueryResult.ContinuationToken;

                    var streams = tableQueryResult.Select(x => x.PartitionKey).Distinct();

                    await Task.WhenAll(streams.Select(x => _queue.EnqueueAsync(x, cancellationToken)));

                } while (continuationToken != null && !cancellationToken.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                // It`s ok.
            }
            catch (StorageException ex) when (ex.IsNotFoundTableNotFound())
            {
                // It`s ok. Nothing pendings.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(0, ex, ex.Message);
            }
        }

        private async Task<TaskSeriesResult> PublishPendingEventsAsync(CancellationToken cancellationToken)
        {
            var stream = await _queue.DequeueAsync(cancellationToken);
            _processing.Add(PublishPendingEventsAsync(stream, cancellationToken));
            return CreateSucceededResult();
        }

        private TaskSeriesResult CreateSucceededResult()
        {
            var wait = WaitForParallelismThreshold();
            return new TaskSeriesResult(wait);
        }

        private async Task WaitForParallelismThreshold()
        {
            while (_processing.Count > _options.MaxDegreeOfParallelism)
            {
                var processed = await Task.WhenAny(_processing);
                _processing.Remove(processed);
            }
        }

        private async Task PublishPendingEventsAsync(string stream, CancellationToken cancellationToken)
        {
            try
            {
                var hasMoreEvents = false;
                do
                {
                    var pendingEvents = await GetPendingEventsAsync(stream, cancellationToken);
                    hasMoreEvents = pendingEvents.HasMoreResults;

                    foreach (var pendingEvent in pendingEvents)
                    {
                        await _messageBus.SendAsync(pendingEvent);
                        if (!await TryDeletePendingEventAsync(pendingEvent))
                        {
                            return;
                        }
                    }
                }
                while (hasMoreEvents && !cancellationToken.IsCancellationRequested);

                await _queue.RemoveAsync(stream, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // It`s ok.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(0, ex, "Publish pending events from stream `{0}` failed.", stream);
                await _queue.RemoveAsync(stream, cancellationToken);
                await _queue.EnqueueAsync(stream, cancellationToken);
            }
        }

        private async Task<Slice<EventData>> GetPendingEventsAsync(string streamName, CancellationToken cancellationToken)
        {
            var condition = GeneratePartitionKeyWithRowKeySliceFilter(
                streamName, UnpublishedRowKeyPrefix, UnpublishedRowKeyPrefixUpperLimit);

            var query = new TableQuery<EventTableEntity>().Where(condition);

            var tableQueryResult = await _table.ExecuteQuerySegmentedAsync(query, null, null, null, cancellationToken);

            return new Slice<EventData>(
                tableQueryResult.Select(x =>
                    EventData.Create(
                        streamName,
                        int.Parse(x.RowKey.Substring(UnpublishedRowKeyPrefix.Length)),
                        JsonConvert.DeserializeObject<IEvent>(x.Payload, SerializerSettings))),
                tableQueryResult.ContinuationToken != null);
        }

        private async Task<bool> TryDeletePendingEventAsync(EventData evt)
        {
            try
            {
                await _table.ExecuteAsync(
                    TableOperation.Delete(
                        new TableEntity(evt.Stream, UnpublishedRowKeyPrefix + GetRowKey(evt.Version)) { ETag = "*" }));
            }
            catch(StorageException ex) when (ex.IsNotFound())
            {
                _logger.LogDebug("Event `{0}` on stream `{1}` already deleted.", evt.Version, evt.Stream);
                return false;
            }
            return true;
        }

        private string GeneratePartitionKeyWithRowKeySliceFilter(string partitionKey, string startRowKey, string endRowKey)
        {
            return TableQuery.CombineFilters(
                GeneratePartitionKeyFilter(partitionKey),
                TableOperators.And,
                GenerateRowKeySliceFilter(startRowKey, endRowKey));
        }

        private string GeneratePartitionKeyFilter(string partitionKey)
        {
            return TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
        }

        private string GenerateRowKeySliceFilter(string startRowKey, string endRowKey)
        {
            var condition1 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startRowKey);
            var condition2 = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, endRowKey);
            return TableQuery.CombineFilters(condition1, TableOperators.And, condition2);
        }

        private string GetRowKey(int version)
        {
            return version.ToString("D10");
        }
    }
}
