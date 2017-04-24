using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Domain
{
    public class ConventionMethodNotFoundException : Exception
    {
        public ConventionMethodNotFoundException() { }
        public ConventionMethodNotFoundException(string message) : base(message) { }
        public ConventionMethodNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
