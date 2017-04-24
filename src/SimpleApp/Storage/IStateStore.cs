using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Storage
{
    public interface IStateStore
    {
        Task<TState> FindByIdAsync<TState>(string key, CancellationToken cancellationToken = default(CancellationToken)) where TState : class;
        Task SaveAsync<TState>(string key, TState state);
    }
}
