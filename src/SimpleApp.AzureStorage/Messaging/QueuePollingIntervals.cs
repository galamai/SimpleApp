using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Messaging
{
    internal static class QueuePollingIntervals
    {
        public static readonly TimeSpan Minimum = TimeSpan.FromMilliseconds(100);
        public static readonly TimeSpan DefaultMaximum = TimeSpan.FromMinutes(1);
    }
}
