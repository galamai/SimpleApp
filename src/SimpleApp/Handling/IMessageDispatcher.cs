using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Handling
{
    public interface IMessageDispatcher
    {
        Task ExecuteAsync(object message, CancellationToken cancellationToken = default(CancellationToken));
    }
}
