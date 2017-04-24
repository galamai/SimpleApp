using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Handling
{
    public class CommandRouter : IMessageRouter<ICommand>
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandRouter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task RouteAsync(ICommand command, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return RouteTypedAsync((dynamic)command, cancellationToken);
        }

        private Task RouteTypedAsync<T>(T command, CancellationToken cancellationToken) where T : ICommand
        {
            var commandType = command.GetType();

            if (_serviceProvider.GetService<IMessageHandler<T>>() is var handler)
            {
                return handler.HandleAsync(command, cancellationToken);
            }

            throw new MissingCommandHandlerException($"Missing handler for command of type `{commandType.Name}`.");
        }
    }
}
