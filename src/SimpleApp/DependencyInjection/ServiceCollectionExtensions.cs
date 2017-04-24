using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleApp;
using SimpleApp.Domain;
using SimpleApp.Handling;
using SimpleApp.Messaging;
using SimpleApp.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static ISimpleAppBuilder AddSimpleApp(this IServiceCollection services)
        {
            return new SimpleAppBuilder(services);
        }
    }
}
