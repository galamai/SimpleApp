using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleApp;
using SimpleApp.AzureStorage.Handling;
using SimpleApp.AzureStorage.Messaging;
using SimpleApp.AzureStorage.Storage;
using SimpleApp.Domain;
using SimpleApp.Handling;
using SimpleApp.Messaging;
using SimpleApp.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SimpleAppBuilderExtensions
    {
        public static ISimpleAppBuilder AddHostTaskAzureQueueStorageReceiver(
            this ISimpleAppBuilder builder,
            string queue,
            string poisonQueue,
            TimeSpan maxPollingInterval,
            string connectionString = "UseDevelopmentStorage=true",
            int batchSize = 16)
        {
            return AddHostTaskAzureQueueStorageReceiver(builder, new MessageReceiverOptions()
            {
                Queue = queue,
                PoisonQueue = poisonQueue,
                MaxPollingInterval = maxPollingInterval,
                ConnectionString = connectionString,
                BatchSize = batchSize
            });
        }

        public static ISimpleAppBuilder AddHostTaskAzureQueueStorageReceiver(this ISimpleAppBuilder builder, MessageReceiverOptions options)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var services = builder.Services;

            services.AddSingleton<IHostTask>(provider => new MessageReceiver(
                options,
                provider.GetRequiredService<IMessageDispatcher>(),
                provider.GetRequiredService<IQueueProcessor>(),
                provider.GetRequiredService<ILogger<MessageReceiver>>()));

            services.AddTransient<IQueueProcessor, QueueProcessor>();

            builder.AddMessageDispatcher();

            return builder;
        }

        public static ISimpleAppBuilder AddMessageSenderAzureQueueStorage(this ISimpleAppBuilder builder, string queue, string connectionString)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return AddMessageSenderAzureQueueStorage(builder, new MessageSenderOptions()
            {
                Queue = queue,
                ConnectionString = connectionString
            });
        }

        public static ISimpleAppBuilder AddMessageSenderAzureQueueStorage(this ISimpleAppBuilder builder, MessageSenderOptions options)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var services = builder.Services;

            services.AddTransient<IMessageSender>(provider => new MessageSender(
                options,
                provider.GetRequiredService<ILogger<MessageSender>>()));

            return builder;
        }

        public static ISimpleAppBuilder AddEventStoreAzureTableStorage(this ISimpleAppBuilder builder, Action<EventStoreOptions> setupAction = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;

            if (setupAction != null)
                services.Configure(setupAction);

            services.AddSingleton<EventStore, EventStore>();
            services.AddTransient<IEventStore>(x => x.GetRequiredService<EventStore>());
            services.AddTransient<IHostTask>(x => x.GetRequiredService<EventStore>());
            services.AddSingleton<IMessageRouter<EventData>, EventDataRouter>();

            return builder;
        }

        public static ISimpleAppBuilder AddBackupEventStoreAzureTableStorage(this ISimpleAppBuilder builder, Action<BackupEventStoreOptions> setupAction = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;

            if (setupAction != null)
                services.Configure(setupAction);

            services.AddSingleton<IBackupEventStore, BackupEventStore>();
            services.AddSingleton<IMessageRouter<EventData>, BackupEventDataRouter>();

            builder.AddMessageDispatcher();

            return builder;
        }

        public static ISimpleAppBuilder AddStateStoreAzureBlobStorage(this ISimpleAppBuilder builder, Action<StateStoreOptions> setupAction = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var services = builder.Services;

            if (setupAction != null)
                services.Configure(setupAction);

            services.AddSingleton<IStateStore, StateStore>();

            return builder;
        }
    }
}
