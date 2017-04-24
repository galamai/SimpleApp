using ConsoleApp1.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Domain
{
    public class User
    {
        public string Name { get; set; }

        private void Apply(UserRegisteredEvent e)
        {
            Name = e.Name;
        }

        private void Apply(UserNameChangedEvent e)
        {
            Name = e.Name;
        }
    }
}
