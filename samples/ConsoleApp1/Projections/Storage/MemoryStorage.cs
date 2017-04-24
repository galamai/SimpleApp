using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Projections.Storage
{
    public class MemoryStorage : IStorage
    {
        private readonly ConcurrentDictionary<Guid, User> _store = new ConcurrentDictionary<Guid, User>();

        public User FindUserById(Guid id)
        {
            if (_store.TryGetValue(id, out User user))
            {
                return user;
            }
            return null;
        }

        public void Save(User user)
        {
            _store.TryAdd(user.Id, user);
        }
    }
}
