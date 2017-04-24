using SimpleApp.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApp.Domain
{
    public static class EventSourcedContextExtensions
    {
        public static Task<bool> TryDuplicateCommitAsync<TId, TState>(this IEventSourcedStreamContext<TId, TState> stream, string correlationId) where TState : class, new()
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return TryDuplicateCommitCoreAsync(stream, correlationId);
        }

        public static Task<bool> TryConcurrencyCommitAsync<TId, TState>(this IEventSourcedStreamContext<TId, TState> stream) where TState : class, new()
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return TryConcurrencyCommitCoreAsync(stream);
        }

        private static async Task<bool> TryDuplicateCommitCoreAsync<TId, TState>(this IEventSourcedStreamContext<TId, TState> stream, string correlationId) where TState : class, new()
        {
            try
            {
                await stream.CommitAsync(correlationId).ConfigureAwait(false);
            }
            catch(DuplicateSaveException)
            {
                return false;
            }

            return true;
        }

        private static async Task<bool> TryConcurrencyCommitCoreAsync<TId, TState>(this IEventSourcedStreamContext<TId, TState> stream) where TState : class, new()
        {
            try
            {
                await stream.CommitAsync().ConfigureAwait(false);
            }
            catch (ConcurrencyException)
            {
                return false;
            }

            return true;
        }
    }
}
