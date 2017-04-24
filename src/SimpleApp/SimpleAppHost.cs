using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp
{
    public class SimpleAppHost : ISimpleAppHost
    {
        private readonly List<IHostTask> _hostTasks;

        public SimpleAppHost(IEnumerable<IHostTask> hostTasks)
        {
            _hostTasks = hostTasks?.ToList() ?? throw new ArgumentNullException(nameof(hostTasks));
        }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(_hostTasks.Select(x => Task.Run(async () => await x.RunAsync(cancellationToken))));
        }
    }
}
