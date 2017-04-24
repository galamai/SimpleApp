using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Messaging
{
    public class MessagaQueueProvider : IMessagaQueueProvider
    {
        private readonly Func<object, string> _provider;

        public MessagaQueueProvider(Func<object, string> provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public string GetQueue(object message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return _provider.Invoke(message);
        }
    }
}
