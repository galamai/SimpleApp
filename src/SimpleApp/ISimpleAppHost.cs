using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp
{
    public interface ISimpleAppHost
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
