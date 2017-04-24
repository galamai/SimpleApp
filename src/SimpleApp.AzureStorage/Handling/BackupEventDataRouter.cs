using SimpleApp.AzureStorage.Storage;
using SimpleApp.Handling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Handling
{
    public class BackupEventDataRouter : IMessageRouter<EventData>
    {
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IBackupEventStore _backupEventStore;

        public BackupEventDataRouter(IMessageDispatcher messageDispatcher, IBackupEventStore backupEventStore)
        {
            _messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
            _backupEventStore = backupEventStore ?? throw new ArgumentNullException(nameof(backupEventStore));
        }

        public Task RouteAsync(EventData message, CancellationToken cancellationToken)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return Task.WhenAll(
                _messageDispatcher.ExecuteAsync(message.Payload, cancellationToken),
                _backupEventStore.BackupAsync(message));
        }
    }
}
