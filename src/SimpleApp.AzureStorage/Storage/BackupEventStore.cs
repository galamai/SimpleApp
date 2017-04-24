using SimpleApp.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using SimpleApp.Storage;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using System.Reflection;
using Newtonsoft.Json;

namespace SimpleApp.AzureStorage.Storage
{
    public class BackupEventStore : IBackupEventStore
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
#if DEBUG
            Formatting = Formatting.Indented
#endif
        };

        private readonly BackupEventStoreOptions _options;
        private readonly CloudTable _table;

        public BackupEventStore(IOptions<BackupEventStoreOptions> optionsAccessor)
        {
            _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));

            var storageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            tableClient.DefaultRequestOptions = new TableRequestOptions()
            {
                PayloadFormat = TablePayloadFormat.Json
            };
            _table = tableClient.GetTableReference(_options.TableName);
        }

        public Task BackupAsync(EventData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            return BackupEventDataAsync(eventData);
        }

        private async Task BackupEventDataAsync(EventData eventData)
        {
            var operation = TableOperation.InsertOrMerge(new EventTableEntity()
            {
                PartitionKey = eventData.Stream,
                RowKey = GetRowKey(eventData.Version),
                Payload = JsonConvert.SerializeObject(eventData.Payload, SerializerSettings),
                Type = eventData.Payload.GetType().Name,
                ETag = "*"
            });

            try
            {
                await _table.ExecuteAsync(operation);
            }
            catch(StorageException ex) when (ex.IsNotFoundTableNotFound())
            {
                await _table.CreateIfNotExistsAsync();
                await _table.ExecuteAsync(operation);
            }
        }

        private string GetRowKey(int version)
        {
            return version.ToString("D10");
        }
    }
}
