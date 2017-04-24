using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Querying
{
    public class MissingQueryException : Exception
    {
        public MissingQueryException() { }
        public MissingQueryException(string message) : base(message) { }
        public MissingQueryException(string message, Exception inner) : base(message, inner) { }
    }
}
