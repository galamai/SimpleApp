using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Messaging
{
    public class MessageSenderOptions
    {
        public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";
        public string Queue { get; set; }
    }
}
