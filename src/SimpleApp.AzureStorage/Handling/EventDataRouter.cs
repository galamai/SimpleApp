using SimpleApp.AzureStorage.Storage;
using SimpleApp.Handling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Handling
{
    public class EventDataRouter : IMessageRouter<EventData>
    {
        private readonly IMessageDispatcher _messageDispatcher;

        public EventDataRouter(IMessageDispatcher messageDispatcher)
        {
            _messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
        }

        public Task RouteAsync(EventData message, CancellationToken cancellationToken)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return _messageDispatcher.ExecuteAsync(message.Payload, cancellationToken);
        }
    }
}
