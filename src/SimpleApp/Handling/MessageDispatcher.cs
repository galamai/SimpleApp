using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Handling
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly ConcurrentDictionary<Type, Type> _routerTypeMap;
        private readonly IServiceProvider _serviceProvider;

        public MessageDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _routerTypeMap = new ConcurrentDictionary<Type, Type>();
        }

        public Task ExecuteAsync(object message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return ExecuteTypedAsync((dynamic)message, cancellationToken);
        }

        private Task ExecuteTypedAsync<T>(T message, CancellationToken cancellationToken)
        {
            IMessageRouter<T> router = null;
            var routerType = _routerTypeMap.GetOrAdd(message.GetType(), messageType =>
            {
                foreach (var implementedType in messageType.GetImplementedTypesAndInterfaces())
                {
                    var implementedRouterType = typeof(IMessageRouter<>).MakeGenericType(implementedType);
                    router = (IMessageRouter<T>)_serviceProvider.GetService(implementedRouterType);
                    if (router != null)
                    {
                        return implementedRouterType;
                    }
                }

                return null;
            });

            if (routerType != null)
            {
                if (router == null)
                {
                    router = (IMessageRouter<T>)_serviceProvider.GetRequiredService(routerType);
                }

                return router.RouteAsync(message, cancellationToken);
            }

            throw new MissingMessageRouterException($"Missing router for message of type `{typeof(T).Name}`.");
        }
    }
}
