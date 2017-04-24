using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SimpleApp.Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Messaging
{
    public class MessageBus : IMessageBus
    {
        private readonly Dictionary<string, IMessageSender> _senders;
        private readonly IMessagaQueueProvider _messagaQueueProvider;

        public MessageBus(IEnumerable<IMessageSender> messageSenders, IMessagaQueueProvider messagaQueueProvider)
        {
            _senders = messageSenders?.ToDictionary(x => x.Name, x => x) ?? throw new ArgumentNullException(nameof(messageSenders));
            _messagaQueueProvider = messagaQueueProvider ?? throw new ArgumentNullException(nameof(messagaQueueProvider));
        }

        public Task SendAsync(object message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var queue = _messagaQueueProvider.GetQueue(message);
            if (_senders.TryGetValue(queue, out var sender))
            {
                return sender.SendAsync(message);
            }

            throw new InvalidOperationException($"Queue for message of type `{message.GetType().Name}` not found.");
        }
    }
}
