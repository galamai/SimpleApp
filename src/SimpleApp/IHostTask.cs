using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp
{
    public interface IHostTask
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
