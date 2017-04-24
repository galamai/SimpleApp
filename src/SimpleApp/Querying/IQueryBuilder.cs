using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Querying
{
    public interface IQueryBuilder
    {
        Task<TResult> ExecuteAsync<TResult>(ICriterion<TResult> criterion, CancellationToken cancellationToken = default(CancellationToken));
    }
}
