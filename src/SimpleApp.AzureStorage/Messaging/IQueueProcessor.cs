using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Messaging
{
    public interface IQueueProcessor
    {
        Task CompleteProcessingMessageAsync(CloudQueue queue, CloudQueueMessage message, CancellationToken cancellationToken);
        Task BadProcessingMessageAsync(CloudQueue queue, CloudQueue poisonQueue, CloudQueueMessage message, CancellationToken cancellationToken);
    }
}
