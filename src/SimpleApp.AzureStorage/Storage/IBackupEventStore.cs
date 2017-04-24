using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Storage
{
    public interface IBackupEventStore
    {
        Task BackupAsync(EventData eventData);
    }
}
