using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Handling
{
    public interface IMessageRouter<in TMessage>
    {
        Task RouteAsync(TMessage message, CancellationToken cancellationToken);
    }
}
