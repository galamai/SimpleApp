using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.Storage.Table
{
    /*public class TableEntityAdapter<T> : TableEntity where T : class, new()
    {
        private T _value;

        public T Value { get { return _value; } }

        public TableEntityAdapter()
            : this(new T())
        {

        }

        public TableEntityAdapter(string partitionKey, string rowKey, T value, string eTag)
            : this(partitionKey, rowKey, value)
        {
            ETag = eTag;
        }

        public TableEntityAdapter(string partitionKey, string rowKey, T value)
            : this(value)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public TableEntityAdapter(T value)
        {
            _value = value;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            ReadUserObject(_value, properties, operationContext);
        }

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return WriteUserObject(_value, operationContext);
        }
    }*/
}
