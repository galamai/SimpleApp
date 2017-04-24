using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Storage.Table
{
    /*public static class CloudTableExtensions
    {
        public static Task<TableResult> ExecuteAsync(this CloudTable table, TableOperation operation, CancellationToken cancellationToken)
        {
            return table.ExecuteAsync(operation, null, null, cancellationToken);
        }

        public static async Task<TableEntityAdapter<T>> RetriveAsync<T>(this CloudTable table, string partitionKey, string rowKey, CancellationToken cancellationToken = default(CancellationToken)) where T : class, new()
        {
            var retrieveOperation = TableOperation.Retrieve<TableEntityAdapter<T>>(partitionKey, rowKey);
            var retrievedResult = await table.ExecuteAsync(retrieveOperation, cancellationToken);
            if (retrievedResult.Result == null)
            {
                return null;
            }
            return (TableEntityAdapter<T>)retrievedResult.Result;
        }
    }*/
}
