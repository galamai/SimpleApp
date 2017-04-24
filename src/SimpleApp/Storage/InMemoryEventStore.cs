using SimpleApp.Handling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Storage
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly ConcurrentDictionary<string, List<IEvent>> _store = new ConcurrentDictionary<string, List<IEvent>>();
        private readonly IMessageDispatcher _messageDispatcher;

        public InMemoryEventStore(IMessageDispatcher messageDispatcher)
        {
            _messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
        }

        public Task<Slice<IEvent>> LoadAsync(string stream, int version, CancellationToken cancellationToken)
        {
            if (_store.TryGetValue(stream, out var events))
            {
                return Task.FromResult(new Slice<IEvent>(
                    events.Skip(version),
                    false
                    ));
            }

            return Task.FromResult(new Slice<IEvent>(new List<IEvent>(), false));
        }

        public async Task SaveAsync(string stream, int expectedVersion, IEnumerable<IEvent> events, string correlationId = null)
        {
            _store.AddOrUpdate(stream, events.ToList(), (x, l) => { l.AddRange(events); return l; });
            foreach (var e in events)
            {
                await _messageDispatcher.ExecuteAsync(e).ConfigureAwait(false);
            }
        }
    }
}
