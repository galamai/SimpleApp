using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Timers
{
    public struct TaskSeriesResult
    {
        public Task Wait { get; }

        public TaskSeriesResult(Task wait)
        {
            Wait = wait;
        }
    }
}
