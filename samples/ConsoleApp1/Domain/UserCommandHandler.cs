using ConsoleApp1.Contracts.Commands;
using ConsoleApp1.Contracts.Events;
using SimpleApp.Domain;
using SimpleApp.Handling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1.Domain
{
    public class UserCommandHandler :
        IMessageHandler<RegisterUserCommand>,
        IMessageHandler<ChangeUserNameCommand>
    {
        private readonly IEventSourced<Guid, User> _eventSourced;

        public UserCommandHandler(IEventSourced<Guid, User> eventSourced)
        {
            _eventSourced = eventSourced ?? throw new ArgumentNullException(nameof(eventSourced));
        }

        public async Task HandleAsync(RegisterUserCommand cmd, CancellationToken cancellationToken)
        {
            using (var stream = await _eventSourced.GetOrCreateStreamAsync(cmd.UserId, cancellationToken))
            {
                if (stream.Version > -1)
                    return;

                stream.RaiseEvent(new UserRegisteredEvent(stream.Id, cmd.Name));
                await stream.TryConcurrencyCommitAsync();
            }
        }

        public async Task HandleAsync(ChangeUserNameCommand cmd, CancellationToken cancellationToken)
        {
            using (var stream = await _eventSourced.GetStreamAsync(cmd.UserId, cancellationToken))
            {
                stream.RaiseEvent(new UserNameChangedEvent(stream.Id, cmd.Name));
                await stream.CommitAsync();
            }
        }
    }
}
