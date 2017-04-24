using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.Querying
{
    public interface IQuery<in TCriterion, TResult> where TCriterion : ICriterion<TResult>
    {
        Task<TResult> ExecuteAsync(TCriterion criterion, CancellationToken cancellationToken);
    }
}
