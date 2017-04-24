using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Handling
{
    public class MissingMessageRouterException : Exception
    {
        public MissingMessageRouterException() { }
        public MissingMessageRouterException(string message) : base(message) { }
        public MissingMessageRouterException(string message, Exception inner) : base(message, inner) { }
    }
}
