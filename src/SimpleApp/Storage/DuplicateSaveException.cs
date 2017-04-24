using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Storage
{
    public class DuplicateSaveException : Exception
    {
        public DuplicateSaveException() { }
        public DuplicateSaveException(string message) : base(message) { }
        public DuplicateSaveException(string message, Exception inner) : base(message, inner) { }
    }
}
