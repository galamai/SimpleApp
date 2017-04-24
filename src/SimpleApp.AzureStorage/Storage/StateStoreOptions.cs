using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Storage
{
    public class StateStoreOptions
    {
        public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";
        public string ContainerName { get; set; } = "states";
    }
}
