using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Domain
{
    class EventSourcedSemaphore<TId, TState> : IDisposable where TState : class, new()
    {
        private readonly SemaphoreSlim _semaphore;

        public EventSourcedStream<TId, TState> Stream { get; set; }

        public bool IsExpired { get; set; }

        public bool CanRemove => _semaphore.CurrentCount == 1;

        public EventSourcedSemaphore()
        {
            _semaphore = new SemaphoreSlim(1);
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            return _semaphore.WaitAsync(cancellationToken);
        }

        public void Release()
        {
            _semaphore.Release();
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        public override bool Equals(object obj)
        {
            if (obj is EventSourcedSemaphore<TId, TState> ess)
            {
                // Hack for (ICollection)ConcurrentDictionary Remove.
                return base.Equals(obj) && CanRemove;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            // Hack for (ICollection)ConcurrentDictionary Remove.
            return base.GetHashCode() ^ CanRemove.GetHashCode();
        }
    }
}
