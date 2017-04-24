using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SimpleApp.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Messaging
{
    public class MessageSender : IMessageSender
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
#if DEBUG
            Formatting = Formatting.Indented
#endif
        };

        private readonly ILogger _logger;
        private readonly CloudQueue _queue;

        public string Name { get; }

        public MessageSender(MessageSenderOptions options, ILogger<MessageSender> logger)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Name = options.Queue;
            var account = CloudStorageAccount.Parse(options.ConnectionString);
            var client = account.CreateCloudQueueClient();
            _queue = client.GetQueueReference(options.Queue);
        }

        public Task SendAsync(object message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return CoreSendAsync(message);
        }

        private async Task CoreSendAsync(object message)
        {
            var json = JsonConvert.SerializeObject(message, SerializerSettings);
            var cloudQueueMessage = new CloudQueueMessage(json);
            try
            {
                await _queue.AddMessageAsync(cloudQueueMessage);
            }
            catch(StorageException ex) when (ex.IsNotFoundQueueNotFound())
            {
                _logger.LogDebug("Queue `{0}` not found. Creating queue.", _queue.Name);

                await _queue.CreateIfNotExistsAsync();
                await _queue.AddMessageAsync(cloudQueueMessage);
            }
        }
    }
}
