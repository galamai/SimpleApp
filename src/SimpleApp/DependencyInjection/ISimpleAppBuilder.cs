using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface ISimpleAppBuilder
    {
        IServiceCollection Services { get; }
    }
}
