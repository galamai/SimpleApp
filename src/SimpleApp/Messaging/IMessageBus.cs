using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.Messaging
{
    public interface IMessageBus
    {
        Task SendAsync(object message);
    }
}
