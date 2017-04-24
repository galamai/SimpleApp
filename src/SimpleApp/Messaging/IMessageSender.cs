using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.Messaging
{
    public interface IMessageSender
    {
        string Name { get; }
        Task SendAsync(object message);
    }
}
