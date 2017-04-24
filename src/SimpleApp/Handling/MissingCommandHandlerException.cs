using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Handling
{
    public class MissingCommandHandlerException : Exception
    {
        public MissingCommandHandlerException() { }
        public MissingCommandHandlerException(string message) : base(message) { }
        public MissingCommandHandlerException(string message, Exception inner) : base(message, inner) { }
    }
}
