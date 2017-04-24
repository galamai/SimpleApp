using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleApp
{
    public sealed class Slice<T> : IEnumerable<T>
    {
        public IEnumerable<T> Results { get; }
        public string ContinuationToken { get; }
        public bool HasMoreResults { get; }

        public Slice(IEnumerable<T> results, string continuationToken)
            : this(results, continuationToken != null)
        {
            ContinuationToken = continuationToken;
        }

        public Slice(IEnumerable<T> results, bool hasMoreResults)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            Results = results.ToList();
            HasMoreResults = hasMoreResults;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Results.GetEnumerator();
        }
    }
}
