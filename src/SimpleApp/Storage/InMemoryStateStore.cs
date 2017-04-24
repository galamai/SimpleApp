using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Storage
{
    public class InMemoryStateStore : IStateStore
    {
        private readonly ConcurrentDictionary<string, object> _store = new ConcurrentDictionary<string, object>();

        public Task<TState> FindByIdAsync<TState>(string key, CancellationToken cancellationToken = default(CancellationToken)) where TState : class
        {
            if (_store.TryGetValue(key, out var value))
            {
                return Task.FromResult((TState)value);
            }
            return Task.FromResult<TState>(null);
        }

        public Task SaveAsync<TState>(string key, TState state)
        {
            _store.AddOrUpdate(key, state, (k, v) => state);

            return Task.CompletedTask;
        }
    }
}
