using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public class SimpleAppBuilder : ISimpleAppBuilder
    {
        private readonly IServiceCollection _services;

        public SimpleAppBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services => _services;
    }
}
