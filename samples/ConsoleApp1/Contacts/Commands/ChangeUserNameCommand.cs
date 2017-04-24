using SimpleApp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Contracts.Commands
{
    public class ChangeUserNameCommand : ICommand
    {
        public Guid UserId { get; }
        public string Name { get; }

        public ChangeUserNameCommand(Guid userId, string name)
        {
            UserId = userId;
            Name = name;
        }
    }
}
