using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Domain
{
    public class UnknownStateException : Exception
    {
        public UnknownStateException() { }
        public UnknownStateException(string message) : base(message) { }
        public UnknownStateException(string message, Exception inner) : base(message, inner) { }
    }
}
