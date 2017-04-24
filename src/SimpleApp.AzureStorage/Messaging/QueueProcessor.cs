using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace SimpleApp.AzureStorage.Messaging
{
    public class QueueProcessor : IQueueProcessor
    {
        private readonly ILogger _logger;
        private readonly QueueProcessorOptions _options;

        public QueueProcessor(ILogger<QueueProcessor> logger, IOptions<QueueProcessorOptions> optionsAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        }

        public Task CompleteProcessingMessageAsync(CloudQueue queue, CloudQueueMessage message, CancellationToken cancellationToken)
        {
            if (queue == null)
                throw new ArgumentNullException(nameof(queue));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return DeleteMessageAsync(queue, message, cancellationToken);
        }

        public Task BadProcessingMessageAsync(CloudQueue queue, CloudQueue poisonQueue, CloudQueueMessage message, CancellationToken cancellationToken)
        {
            if (queue == null)
                throw new ArgumentNullException(nameof(queue));

            if (poisonQueue == null)
                throw new ArgumentNullException(nameof(poisonQueue));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.DequeueCount >= _options.MaxDequeueCount)
            {
                return MoveMessageToPoisonQueueAsync(queue, poisonQueue, message, cancellationToken);
            }
            else
            {
                return ReleaseMessageAsync(queue, message, cancellationToken);
            }
        }

        private async Task ReleaseMessageAsync(CloudQueue queue, CloudQueueMessage message, CancellationToken cancellationToken)
        {
            try
            {
                await queue.UpdateMessageAsync(message, _options.BadProcessingVisibilityTimeout, MessageUpdateFields.Visibility, null, null, cancellationToken);
            }
            catch (StorageException ex) when (
                ex.IsBadRequestPopReceiptMismatch() ||
                ex.IsNotFoundMessageOrQueueNotFound() ||
                ex.IsConflictQueueBeingDeletedOrDisabled())
            {
                // It`s ok;
            }
        }

        private async Task MoveMessageToPoisonQueueAsync(CloudQueue queue, CloudQueue poisonQueue, CloudQueueMessage message, CancellationToken cancellationToken)
        {
            await CopyMessageToPoisonQueueAsync(poisonQueue, message, cancellationToken);
            await DeleteMessageAsync(queue, message, cancellationToken);
        }

        private async Task CopyMessageToPoisonQueueAsync(CloudQueue poisonQueue, CloudQueueMessage message, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Message has reached MaxDequeueCount of {0}. Moving message to queue '{1}'.", _options.MaxDequeueCount, poisonQueue.Name);

            try
            {
                await poisonQueue.AddMessageAsync(message);
            }
            catch(StorageException ex) when (ex.IsNotFoundQueueNotFound())
            {
                await poisonQueue.CreateIfNotExistsAsync(null, null, cancellationToken);
                await poisonQueue.AddMessageAsync(message);
            }
        }

        private async Task DeleteMessageAsync(CloudQueue queue, CloudQueueMessage message, CancellationToken cancellationToken)
        {
            try
            {
                await queue.DeleteMessageAsync(message);
            }
            catch (StorageException ex) when (
                ex.IsBadRequestPopReceiptMismatch() ||
                ex.IsNotFoundMessageOrQueueNotFound() ||
                ex.IsConflictQueueBeingDeletedOrDisabled())
            {
                return;
            }
        }
    }
}
