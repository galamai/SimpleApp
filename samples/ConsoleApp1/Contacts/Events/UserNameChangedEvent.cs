using SimpleApp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Contracts.Events
{
    public class UserNameChangedEvent : IEvent
    {
        public Guid UserId { get; }
        public string Name { get; }

        public UserNameChangedEvent(Guid userId, string name)
        {
            UserId = userId;
            Name = name;
        }
    }
}
