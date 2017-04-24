using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Handling
{
    public interface IMessageHandler<in T>
    {
        Task HandleAsync(T message, CancellationToken cancellationToken);
    }
}
