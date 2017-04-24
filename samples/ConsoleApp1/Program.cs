using ConsoleApp1.Contracts.Commands;
using ConsoleApp1.Domain;
using ConsoleApp1.Projections;
using ConsoleApp1.Projections.Queries;
using ConsoleApp1.Projections.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleApp;
using SimpleApp.Messaging;
using SimpleApp.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();

            services.AddSimpleApp()
                .AddSimpleAppHost(addInfiniteHostTask: true)
                .AddEventSourced()
                .AddMessageDispatcher()
                .AddMessageBus()
                .AddQueryBuilder()
                .AddMessageHandler<UserCommandHandler>()
                .AddMessageHandler<UserEventHandler>()
                .AddQuery<UserQuery>();

            services.AddSingleton<IStorage, MemoryStorage>();

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug)
                .AddDebug();

            var host = provider.GetService<ISimpleAppHost>();
            var task = host.RunAsync(CancellationToken.None);

            var queryBuilder = provider.GetService<IQueryBuilder>();
            var messageBus = provider.GetService<IMessageBus>();

            var maxDegreeOfParallelism = 8;
            var idsCount = 10;
            var changesCount = 100;
            var sending = new List<Task>();
            var sendingCount = 0;

            var ids = Enumerable.Range(0, idsCount).Select(x => Guid.NewGuid()).ToList();
            foreach (var id in ids)
            {
                Interlocked.Increment(ref sendingCount);
                sending.Add(messageBus.SendAsync(new RegisterUserCommand(id, "UserName")));
                await WaitDegreeOfParallelismAsync(sending, maxDegreeOfParallelism).ConfigureAwait(false);
            }

            for (int i = 0; i < changesCount; i++)
            {
                foreach(var id in ids)
                {
                    Interlocked.Increment(ref sendingCount);
                    sending.Add(messageBus.SendAsync(new ChangeUserNameCommand(id, Guid.NewGuid().ToString())));
                    await WaitDegreeOfParallelismAsync(sending, maxDegreeOfParallelism).ConfigureAwait(false);
                }
            }

            await Task.WhenAll(sending).ConfigureAwait(false);

            Console.WriteLine($"Commands sended {sendingCount}.");

            var user = await queryBuilder.ExecuteAsync(new FindUserById(ids.First()));
            Console.WriteLine($"User by id `{user.Id}` and name `{user.Name}` found.");

            await task.ConfigureAwait(false);
        }

        static async Task WaitDegreeOfParallelismAsync(List<Task> sending, int maxDegreeOfParallelism)
        {
            while (sending.Count >= maxDegreeOfParallelism)
            {
                var sended = await Task.WhenAny(sending).ConfigureAwait(false);
                sending.Remove(sended);
            }
        }
    }
}