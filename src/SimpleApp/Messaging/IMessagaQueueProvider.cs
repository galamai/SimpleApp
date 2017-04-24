using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Messaging
{
    public interface IMessagaQueueProvider
    {
        string GetQueue(object message);
    }
}
