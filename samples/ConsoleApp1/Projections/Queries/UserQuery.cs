using SimpleApp.Querying;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp1.Projections.Storage;

namespace ConsoleApp1.Projections.Queries
{
    public class UserQuery : IQuery<FindUserById, User>
    {
        private readonly IStorage _storage;

        public UserQuery(IStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public Task<User> ExecuteAsync(FindUserById criterion, CancellationToken cancellationToken)
        {
            return Task.FromResult(_storage.FindUserById(criterion.UserId));
        }
    }
}
