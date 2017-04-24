using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Domain
{
    public class EventSourcedOptions
    {
        public bool ThrowOnConventionMethodNotFound { get; set; } = false;
        public int SaveStateStep { get; set; } = 100;
    }
}
