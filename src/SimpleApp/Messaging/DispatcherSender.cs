using SimpleApp.Handling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.Messaging
{
    public class DispatcherSender : IMessageSender
    {
        private readonly IMessageDispatcher _messageDispatcher;

        public string Name => "MessageDispatcher";

        public DispatcherSender(IMessageDispatcher messageDispatcher)
        {
            _messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
        }

        public Task SendAsync(object message)
        {
            return _messageDispatcher.ExecuteAsync(message);
        }
    }
}
