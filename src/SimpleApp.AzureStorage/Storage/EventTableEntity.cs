using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Storage
{
    public class EventTableEntity : TableEntity
    {
        public string Payload { get; set; }
        public string Type { get; set; }
        public string CorrelationId { get; set; }
    }
}
