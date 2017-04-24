using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleApp.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Domain
{
    public class EventSourced<TId, TState> : IEventSourced<TId, TState> where TState : class, new()
    {
        private readonly IEventStore _eventStore;
        private readonly IStateStore _stateStore;
        private readonly IOptions<EventSourcedOptions> _optionsAccessor;
        private readonly ConcurrentDictionary<TId, EventSourcedSemaphore<TId, TState>> _eventSourcedSemaphores;

        public EventSourced(IEventStore eventStore, IStateStore stateStore, IOptions<EventSourcedOptions> optionsAccessor)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
            _optionsAccessor = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));
            _eventSourcedSemaphores = new ConcurrentDictionary<TId, EventSourcedSemaphore<TId, TState>>();
        }

        public async Task<IEventSourcedStreamContext<TId, TState>> GetOrCreateStreamAsync(TId id, CancellationToken cancellationToken)
        {
            var eventSourcedSemaphore = _eventSourcedSemaphores.GetOrAdd(id, key => new EventSourcedSemaphore<TId, TState>());
            await eventSourcedSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (eventSourcedSemaphore.Stream == null)
            {
                eventSourcedSemaphore.Stream = new EventSourcedStream<TId, TState>(id);
            }
            if (eventSourcedSemaphore.IsExpired)
            {
                eventSourcedSemaphore.Stream = await GetEventSourcedStreamAsync(id, cancellationToken).ConfigureAwait(false);
            }
            return new EventSourcedStreamContext<TId, TState>(this, eventSourcedSemaphore);
        }

        public async Task<IEventSourcedStreamContext<TId, TState>> GetStreamAsync(TId id, CancellationToken cancellationToken)
        {
            var eventSourcedSemaphore = _eventSourcedSemaphores.GetOrAdd(id, key => new EventSourcedSemaphore<TId, TState>());
            await eventSourcedSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (eventSourcedSemaphore.Stream == null || eventSourcedSemaphore.IsExpired)
            {
                eventSourcedSemaphore.Stream = await GetEventSourcedStreamAsync(id, cancellationToken).ConfigureAwait(false);
            }
            return new EventSourcedStreamContext<TId, TState>(this, eventSourcedSemaphore);
        }

        internal void ReleaseEventSourcedSemaphore(EventSourcedSemaphore<TId, TState> eventSourcedSemaphore)
        {
            eventSourcedSemaphore.Release();

            var collection = (ICollection<KeyValuePair<TId, EventSourcedSemaphore<TId, TState>>>)_eventSourcedSemaphores;
            if (collection.Remove(new KeyValuePair<TId, EventSourcedSemaphore<TId, TState>>(eventSourcedSemaphore.Stream.Id, eventSourcedSemaphore)))
            {
                eventSourcedSemaphore.Dispose();
            }
        }

        internal async Task CommitAsync(EventSourcedSemaphore<TId, TState> eventSourcedSemaphore, string correlationId = null)
        {
            var eventSourcedStream = eventSourcedSemaphore.Stream;
            var streamName = GetStreamName(eventSourcedStream.Id);

            try
            {
                await _eventStore.SaveAsync(
                    streamName,
                    eventSourcedStream.Version - eventSourcedStream.GetUncommitedEvents().Count(),
                    eventSourcedStream.GetUncommitedEvents(),
                    correlationId).ConfigureAwait(false);
            }
            catch(ConcurrencyException)
            {
                eventSourcedSemaphore.IsExpired = true;
                throw;
            }
            catch(DuplicateSaveException)
            {
                eventSourcedSemaphore.IsExpired = true;
                throw;
            }

            await SaveStateAsync(streamName, eventSourcedStream).ConfigureAwait(false);

            eventSourcedStream.ClearUncommitedEvents();
        }

        private async Task<EventSourcedStream<TId, TState>> GetEventSourcedStreamAsync(TId id, CancellationToken cancellationToken = default(CancellationToken))
        {
            var eventSourced = await LoadEventSourcedStreamAsync(id, cancellationToken).ConfigureAwait(false);

            if (_optionsAccessor.Value.ThrowOnConventionMethodNotFound)
            {
                eventSourced.ThrowOnConventionMethodNotFound();
            }

            await LoadEventsAsync(eventSourced, cancellationToken).ConfigureAwait(false);

            if (eventSourced.Version == -1)
            {
                throw new UnknownStateException($"State of type `{typeof(TState).Name}` by id `{id}` unknown.");
            }

            return eventSourced;
        }

        private async Task<EventSourcedStream<TId, TState>> LoadEventSourcedStreamAsync(TId id, CancellationToken cancellationToken)
        {
            var streamName = GetStreamName(id);

            return await _stateStore.FindByIdAsync<EventSourcedStream<TId, TState>>(streamName, cancellationToken)
                .ConfigureAwait(false) ?? new EventSourcedStream<TId, TState>(id);
        }

        private async Task LoadEventsAsync(EventSourcedStream<TId, TState> state, CancellationToken cancellationToken)
        {
            var streamName = GetStreamName(state.Id);

            var hasMoreEvents = false;
            do
            {
                var events = await _eventStore.LoadAsync(streamName, state.Version + 1, cancellationToken).ConfigureAwait(false);
                state.ApplyEvents(events);
                hasMoreEvents = events.HasMoreResults;

                if (hasMoreEvents)
                {
                    await SaveStateAsync(streamName, state).ConfigureAwait(false);
                }

            } while (hasMoreEvents);
        }

        private string GetStreamName(TId id)
        {
            return $"{typeof(TState).Name}_{id}";
        }

        private Task SaveStateAsync(string stream, EventSourcedStream<TId, TState> eventSourced)
        {
            if (eventSourced.Version >= _optionsAccessor.Value.SaveStateStep &&
                eventSourced.Version % _optionsAccessor.Value.SaveStateStep - eventSourced.GetUncommitedEvents().Count() < 0)
            {
                return _stateStore.SaveAsync(stream, eventSourced);
            }

            return Task.CompletedTask;
        }
    }
}
