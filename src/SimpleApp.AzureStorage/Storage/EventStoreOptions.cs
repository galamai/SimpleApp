using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SimpleApp.AzureStorage.Storage
{
    public class EventStoreOptions
    {
        public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";
        public string TableName { get; set; } = "EventStore";
        public int MaxDegreeOfParallelism { get; set; } = 32;
    }
}
