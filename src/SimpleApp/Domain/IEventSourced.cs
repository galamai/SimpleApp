using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Domain
{
    public interface IEventSourced<TId, TState> where TState : class, new()
    {
        Task<IEventSourcedStreamContext<TId, TState>> GetOrCreateStreamAsync(TId id, CancellationToken cancellationToken);
        Task<IEventSourcedStreamContext<TId, TState>> GetStreamAsync(TId id, CancellationToken cancellationToken);
    }
}
