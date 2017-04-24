using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SimpleApp.AzureStorage.Timers;
using SimpleApp.Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Messaging
{
    public class MessageReceiver : IHostTask
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
#if DEBUG
            Formatting = Formatting.Indented
#endif
        };

        private readonly MessageReceiverOptions _options;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IQueueProcessor _queueProcessor;
        private readonly ILogger _logger;
        private readonly List<Task> _processing;
        private readonly CloudQueue _queue;
        private readonly CloudQueue _poisonQueue;
        private readonly TimeSpan _visibilityTimeout;
        private readonly IDelayStrategy _delayStrategy;

        private bool _foundMessageSinceLastDelay;

        public MessageReceiver(
            MessageReceiverOptions options,
            IMessageDispatcher messageDispatcher,
            IQueueProcessor queueProcessor,
            ILogger<MessageReceiver> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
            _queueProcessor = queueProcessor ?? throw new ArgumentNullException(nameof(queueProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processing = new List<Task>();

            var account = CloudStorageAccount.Parse(_options.ConnectionString);
            var client = account.CreateCloudQueueClient();
            _queue = client.GetQueueReference(_options.Queue);
            _poisonQueue = client.GetQueueReference(_options.PoisonQueue);

            _visibilityTimeout = TimeSpan.FromMinutes(10);
            _delayStrategy = new RandomizedDelayStrategy(QueuePollingIntervals.Minimum, _options.MaxPollingInterval); ;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using (var timer = TaskSeriesTimer.StartNew(ExecuteAsync, _logger))
            {
                await timer.WaitAsync(cancellationToken);
            }
        }

        private async Task<TaskSeriesResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!await _queue.ExistsAsync(null, null, cancellationToken))
            {
                return CreateBackoffResult();
            }

            IEnumerable<CloudQueueMessage> messages = null;
            try
            {
                messages = await _queue.GetMessagesAsync(
                    _options.BatchSize,
                    _visibilityTimeout,
                    null,
                    null,
                    cancellationToken);
            }
            catch (StorageException ex) when (
                ex.IsNotFoundQueueNotFound() ||
                ex.IsConflictQueueBeingDeletedOrDisabled() ||
                ex.IsServerSideError())
            {
                _logger.LogWarning(0, ex, "Get messages from queue `{0}` failed.", _queue.Name);
                return CreateBackoffResult();
            }

            if (messages == null)
            {
                return CreateBackoffResult();
            }

            var fountMessages = messages.Where(x => x != null);

            if (!fountMessages.Any())
            {
                return CreateBackoffResult();
            }

            _processing.AddRange(fountMessages.Select(x => ProcessMessageAsync(x, cancellationToken)));

            _foundMessageSinceLastDelay = true;

            return CreateSucceededResult();
        }

        private async Task ProcessMessageAsync(CloudQueueMessage message, CancellationToken cancellationToken)
        {
            try
            {
                using (var timer = StartNewUpdateMessageVisibilityTimer(_queue, message, _visibilityTimeout, _logger))
                {
                    var payload = JsonConvert.DeserializeObject<object>(message.AsString, SerializerSettings);
                    await _messageDispatcher.ExecuteAsync(payload, cancellationToken);
                    await timer.StopAsync(cancellationToken);
                }

                await _queueProcessor.CompleteProcessingMessageAsync(_queue, message, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // It`s ok.
            }
            catch (Exception ex)
            {
                _logger.LogWarning(0, ex, ex.Message);
                await _queueProcessor.BadProcessingMessageAsync(_queue, _poisonQueue, message, cancellationToken);
            }
        }

        private TaskSeriesResult CreateSucceededResult()
        {
            var wait = WaitForNewBatchThreshold();
            return new TaskSeriesResult(wait);
        }

        private TaskSeriesResult CreateBackoffResult()
        {
            _foundMessageSinceLastDelay = false;
            var nextDelay = _delayStrategy.GetNextDelay(_foundMessageSinceLastDelay);
            var delay = Task.Delay(nextDelay);
            return new TaskSeriesResult(delay);
        }

        private async Task WaitForNewBatchThreshold()
        {
            while (_processing.Count > _options.NewBatchThreshold)
            {
                var processed = await Task.WhenAny(_processing);
                _processing.Remove(processed);
            }
        }

        private TaskSeriesTimer StartNewUpdateMessageVisibilityTimer(
            CloudQueue queue,
            CloudQueueMessage message,
            TimeSpan visibilityTimeout,
            ILogger logger)
        {
            var normalUpdateInterval = new TimeSpan(visibilityTimeout.Ticks / 2);
            var delayStrategy = new LinearDelayStrategy(normalUpdateInterval, TimeSpan.FromMinutes(1));
            var visibilityProcessor = new QueueMessageVisibilityProcessor(queue, message, visibilityTimeout, delayStrategy);
            return TaskSeriesTimer.StartNew(visibilityProcessor.UpdateAsync, logger, Task.Delay(normalUpdateInterval));
        }
    }
}
