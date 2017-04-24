using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.AzureStorage.Storage
{
    public class EventData
    {
        public string Stream { get; }
        public int Version { get; }
        public IEvent Payload { get; }

        public EventData(string stream, int version, IEvent payload)
        {
            Stream = stream;
            Version = version;
            Payload = payload;
        }

        public static EventData Create(string stream, int version, IEvent payload)
        {
            return new EventData(stream, version, payload);
        }
    }
}
