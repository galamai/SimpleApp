using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Querying
{
    public class QueryBuilder : IQueryBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task<TResult> ExecuteAsync<TResult>(ICriterion<TResult> criterion, CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryType = typeof(IQuery<,>).MakeGenericType(criterion.GetType(), typeof(TResult));
            var query = _serviceProvider.GetService(queryType);

            if (query == null)
                throw new MissingQueryException($"Missing query for criterion of type `{criterion.GetType().Name}`.");

            return ((dynamic)query).ExecuteAsync((dynamic)criterion, cancellationToken);
        }
    }
}
