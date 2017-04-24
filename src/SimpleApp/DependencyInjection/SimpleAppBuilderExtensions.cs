using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleApp;
using SimpleApp.Domain;
using SimpleApp.Handling;
using SimpleApp.Messaging;
using SimpleApp.Querying;
using SimpleApp.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SimpleAppBuilderExtensions
    {
        public static ISimpleAppBuilder AddSimpleAppHost(this ISimpleAppBuilder builder, bool addInfiniteHostTask = false)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;

            services.AddSingleton<ISimpleAppHost, SimpleAppHost>();

            if (addInfiniteHostTask)
            {
                services.AddSingleton<IHostTask, InfiniteHostTask>();
            }

            return builder;
        }

        public static ISimpleAppBuilder AddHostTask<T>(this ISimpleAppBuilder builder) where T : class, IHostTask
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;

            services.AddTransient<IHostTask, T>();

            return builder;
        }

        public static ISimpleAppBuilder AddEventSourced(this ISimpleAppBuilder builder, Action<EventSourcedOptions> setupAction = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;

            services.AddSingleton(typeof(IEventSourced<,>), typeof(EventSourced<,>));
            services.TryAddSingleton<IEventStore, InMemoryEventStore>();
            services.TryAddSingleton<IStateStore, InMemoryStateStore>();

            builder.AddMessageDispatcher();

            return builder;
        }

        public static ISimpleAppBuilder AddMessageDispatcher(this ISimpleAppBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;

            services.TryAddSingleton<IMessageDispatcher, MessageDispatcher>();
            services.TryAddSingleton<IMessageRouter<ICommand>, CommandRouter>();
            services.TryAddSingleton<IMessageRouter<IEvent>, EventRouter>();

            return builder;
        }

        public static ISimpleAppBuilder AddMessageHandler<T>(this ISimpleAppBuilder builder)
        {
            return AddMessageHandler(builder, typeof(T));
        }

        public static ISimpleAppBuilder AddMessageHandler(this ISimpleAppBuilder builder, Type implementationType)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            var services = builder.Services;

            var interfaces = implementationType
                .GetTypeInfo()
                .ImplementedInterfaces
                .Where(x =>
                    x.GetTypeInfo().IsGenericType &&
                    x.GetTypeInfo().GetGenericTypeDefinition() == typeof(IMessageHandler<>));

            foreach (var serviceType in interfaces)
            {
                services.AddTransient(serviceType, implementationType);
            }

            return builder;
        }

        public static ISimpleAppBuilder AddQueryBuilder(this ISimpleAppBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;

            services.AddSingleton<IQueryBuilder, QueryBuilder>();

            return builder;
        }

        public static ISimpleAppBuilder AddQuery<TQuery>(this ISimpleAppBuilder builder)
        {
            return AddQuery(builder, typeof(TQuery));
        }

        public static ISimpleAppBuilder AddQuery(this ISimpleAppBuilder builder, Type implementationType)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (implementationType == null)
                throw new ArgumentNullException(nameof(implementationType));

            var services = builder.Services;

            var interfaces = implementationType
                .GetTypeInfo()
                .ImplementedInterfaces
                .Where(x => x.GetTypeInfo().IsGenericType && (x.GetTypeInfo().GetGenericTypeDefinition() == typeof(IQuery<,>)));

            foreach (var serviceType in interfaces)
            {
                services.AddTransient(serviceType, implementationType);
            }

            return builder;
        }

        public static ISimpleAppBuilder AddMessageBus(this ISimpleAppBuilder builder, Func<object, string> messagaQueueProvider = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;

            services.TryAddSingleton<IMessageBus, MessageBus>();

            var queueProvider = new MessagaQueueProvider(messagaQueueProvider ?? new Func<object, string>(message => "MessageDispatcher"));
            services.TryAddTransient<IMessagaQueueProvider>(serviceProvider => queueProvider);

            services.AddTransient<IMessageSender, DispatcherSender>();

            builder.AddMessageDispatcher();

            return builder;
        }
    }
}
