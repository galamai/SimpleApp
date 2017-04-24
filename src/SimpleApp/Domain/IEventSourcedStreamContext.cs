using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.Domain
{
    public interface IEventSourcedStreamContext<TId, TState> : IDisposable where TState : class, new()
    {
        TId Id { get; }
        int Version { get; }
        TState State { get; }
        void RaiseEvent(IEvent e);
        Task CommitAsync(string correlationId = null);
    }
}
