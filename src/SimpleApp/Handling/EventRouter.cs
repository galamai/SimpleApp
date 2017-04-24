using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Handling
{
    public class EventRouter : IMessageRouter<IEvent>
    {
        private readonly ConcurrentDictionary<Type, List<Type>> _handlerTypeMap;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public EventRouter(IServiceProvider serviceProvider, ILogger<EventRouter> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _handlerTypeMap = new ConcurrentDictionary<Type, List<Type>>();
        }

        public Task RouteAsync(IEvent evt, CancellationToken cancellationToken)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            return RouteTypedAsync((dynamic)evt, cancellationToken);
        }

        private Task RouteTypedAsync<T>(T evt, CancellationToken cancellationToken) where T : IEvent
        {
            List<IMessageHandler<T>> handlers = new List<IMessageHandler<T>>();
            var handlerTypes = _handlerTypeMap.GetOrAdd(evt.GetType(), eventType =>
            {
                var implementedHandlerTypes = new List<Type>();
                foreach (var implementedType in eventType.GetImplementedTypesAndInterfaces().Where(x => typeof(IEvent).GetTypeInfo().IsAssignableFrom(x.GetTypeInfo())))
                {
                    var handlerType = typeof(IMessageHandler<>).MakeGenericType(implementedType);
                    if (_serviceProvider.GetService(handlerType) is IMessageHandler<T> handler)
                    {
                        implementedHandlerTypes.Add(handlerType);
                        handlers.Add(handler);
                    }
                }

                return implementedHandlerTypes;
            });

            if (!handlerTypes.Any())
            {
                _logger.LogDebug($"Missing handlers for event of type `{evt.GetType().Name}`.");
                return Task.CompletedTask;
            }

            if (handlerTypes.Count != handlers.Count)
            {
                handlers.AddRange(handlerTypes.Select(x => (IMessageHandler<T>)_serviceProvider.GetService(x)));
            }

            return Task.WhenAll(handlers.Select(x => x.HandleAsync(evt, cancellationToken)));
        }
    }
}
