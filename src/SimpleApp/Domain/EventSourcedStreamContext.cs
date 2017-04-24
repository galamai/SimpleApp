using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.Domain
{
    class EventSourcedStreamContext<TId, TState> : IEventSourcedStreamContext<TId, TState>, IDisposable where TState : class, new()
    {
        private readonly EventSourced<TId, TState> _eventSourced;
        private readonly EventSourcedSemaphore<TId, TState> _eventSourcedSemaphore;

        private bool _disposed;

        public TId Id
        {
            get
            {
                ThrowIfDisposed();
                return _eventSourcedSemaphore.Stream.Id;
            }
        }

        public int Version
        {
            get
            {
                ThrowIfDisposed();
                return _eventSourcedSemaphore.Stream.Version;
            }
        }

        public TState State
        {
            get
            {
                ThrowIfDisposed();
                return _eventSourcedSemaphore.Stream.State;
            }
        }

        public EventSourcedStreamContext(EventSourced<TId, TState> eventSourced, EventSourcedSemaphore<TId, TState> eventSourcedSemaphore)
        {
            _eventSourced = eventSourced ?? throw new ArgumentNullException(nameof(eventSourced));
            _eventSourcedSemaphore = eventSourcedSemaphore ?? throw new ArgumentNullException(nameof(eventSourcedSemaphore));
        }

        public void RaiseEvent(IEvent e)
        {
            ThrowIfDisposed();
            _eventSourcedSemaphore.Stream.RaiseEvent(e);
        }

        public Task CommitAsync(string correlationId = null)
        {
            return _eventSourced.CommitAsync(_eventSourcedSemaphore, correlationId);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _eventSourced.ReleaseEventSourcedSemaphore(_eventSourcedSemaphore);
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}
