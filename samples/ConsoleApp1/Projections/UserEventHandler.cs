using ConsoleApp1.Contracts.Events;
using ConsoleApp1.Projections.Storage;
using SimpleApp.Handling;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1.Projections
{
    public class UserEventHandler :
        IMessageHandler<UserRegisteredEvent>,
        IMessageHandler<UserNameChangedEvent>
    {
        private readonly IStorage _storage;

        public UserEventHandler(IStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public Task HandleAsync(UserRegisteredEvent evt, CancellationToken cancellationToken)
        {
            var user = new User()
            {
                Id = evt.UserId,
                Name = evt.Name
            };
            _storage.Save(user);

            Console.WriteLine($"User `{evt.UserId}` registered.");

            return Task.CompletedTask;
        }

        public Task HandleAsync(UserNameChangedEvent evt, CancellationToken cancellationToken)
        {
            var user = _storage.FindUserById(evt.UserId);
            user.Name = evt.Name;
            _storage.Save(user);

            Console.WriteLine($"User `{evt.UserId}` name changed.");

            return Task.CompletedTask;
        }
    }
}
