using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SimpleApp.Domain
{
    class EventSourcedStream<TId, TState> where TState : class, new()
    {
        private const string ConventionMethodName = "Apply";
        private static readonly IDictionary<Type, Delegate> _routes;

        private readonly List<IEvent> _uncommitedEvents = new List<IEvent>();
        private bool _throwOnConventionMethodNotFound;

        public TId Id { get; }
        public int Version { get; private set; }
        public TState State { get; }

        static EventSourcedStream()
        {
            _routes = typeof(TState)
                .GetRuntimeMethods()
                .Where(m =>
                    m.Name == ConventionMethodName &&
                    m.GetParameters().Length == 1 &&
                    m.ReturnParameter.ParameterType == typeof(void))
                .ToDictionary(m =>
                    m.GetParameters().Single().ParameterType,
                    m => m.CreateDelegate(typeof(Action<,>).MakeGenericType(typeof(TState), m.GetParameters().Single().ParameterType)));
        }

        public EventSourcedStream(TId id, int version = -1, TState value = null)
        {
            Id = id;
            Version = version;
            State = value ?? new TState();
        }

        public void RaiseEvent(IEvent e)
        {
            ApplyEvent(e);
            _uncommitedEvents.Add(e);
        }

        public void ThrowOnConventionMethodNotFound()
        {
            _throwOnConventionMethodNotFound = true;
        }

        public void ApplyEvents(IEnumerable<IEvent> events)
        {
            foreach (var e in events)
            {
                ApplyEvent(e);
            }
        }

        public IEnumerable<IEvent> GetUncommitedEvents() => _uncommitedEvents;

        public void ClearUncommitedEvents() => _uncommitedEvents.Clear();

        private void ApplyEvent(IEvent e)
        {
            if (_routes.TryGetValue(e.GetType(), out var action))
            {
                ((dynamic)action).Invoke(State, (dynamic)e);
            }
            else if(_throwOnConventionMethodNotFound)
            {
                throw new ConventionMethodNotFoundException(
                    $"Aggregate of type `{State.GetType().Name}` raised an event of type `{e.GetType().Name}` " +
                    $"but not handler could be found to handle the event.");
            }
            else
            {
                Debug.WriteLine($"Convention method on state of type `{State.GetType().Name}` for event of type `{e.GetType().Name}` not found.");
            }
            Version++;
        }
    }
}
