using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Storage
{
    public interface IEventStore
    {
        Task<Slice<IEvent>> LoadAsync(string streamName, int version, CancellationToken cancellationToken);
        Task SaveAsync(string streamName, int expectedVersion, IEnumerable<IEvent> events, string correlationId = null);
    }
}
