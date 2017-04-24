using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Timers
{
    public interface IDelayStrategy
    {
        TimeSpan GetNextDelay(bool executionSucceeded);
    }
}
