using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Storage
{
    public class BackupEventStoreOptions
    {
        public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";
        public string TableName { get; set; } = "BackupEventStore";
    }
}
