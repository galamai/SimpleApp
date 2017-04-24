using SimpleApp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Contracts.Commands
{
    public class RegisterUserCommand : ICommand
    {
        public Guid UserId { get; }
        public string Name { get; }
        
        public RegisterUserCommand(Guid userId, string name)
        {
            UserId = userId;
            Name = name;
        }
    }
}
