using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using SimpleApp.AzureStorage.Timers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Messaging
{
    internal class QueueMessageVisibilityProcessor
    {
        private readonly CloudQueue _queue;
        private readonly CloudQueueMessage _message;
        private readonly TimeSpan _visibilityTimeout;
        private readonly IDelayStrategy _delayStrategy;

        public QueueMessageVisibilityProcessor(
            CloudQueue queue,
            CloudQueueMessage message,
            TimeSpan visibilityTimeout,
            IDelayStrategy delayStrategy)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _visibilityTimeout = visibilityTimeout;
            _delayStrategy = delayStrategy ?? throw new ArgumentNullException(nameof(delayStrategy));
        }

        public async Task<TaskSeriesResult> UpdateAsync(CancellationToken cancellationToken)
        {
            TimeSpan delay;

            try
            {
                await _queue.UpdateMessageAsync(
                    _message,
                    _visibilityTimeout,
                    MessageUpdateFields.Visibility,
                    null,
                    null,
                    cancellationToken);

                delay = _delayStrategy.GetNextDelay(executionSucceeded: true);
            }
            catch(StorageException ex) when (ex.IsServerSideError())
            {
                delay = _delayStrategy.GetNextDelay(executionSucceeded: false);
            }
            catch(StorageException ex) when (
                ex.IsBadRequestPopReceiptMismatch() ||
                ex.IsNotFoundMessageOrQueueNotFound() ||
                ex.IsConflictQueueBeingDeletedOrDisabled())
            {
                delay = Timeout.InfiniteTimeSpan;
            }

            return new TaskSeriesResult(Task.Delay(delay));
        }
    }
}
