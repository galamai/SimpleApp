using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Projections.Storage
{
    public interface IStorage
    {
        User FindUserById(Guid id);
        void Save(User user);
    }
}
