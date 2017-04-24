using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Messaging
{
    public class QueueProcessorOptions
    {
        public int MaxDequeueCount { get; set; } = 5;
        public TimeSpan BadProcessingVisibilityTimeout { get; set; } = TimeSpan.Zero;
    }
}
