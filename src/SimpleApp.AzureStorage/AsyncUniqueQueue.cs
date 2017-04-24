using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage
{
    enum UniqueQueueSession : byte
    {
        Unique,
        RestartAfterRemove
    }

    public sealed class AsyncUniqueQueue<T>
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        private readonly Queue<T> _producerQueue = new Queue<T>();
        private readonly Queue<TaskCompletionSource<T>> _consumerQueue = new Queue<TaskCompletionSource<T>>();
        private readonly Dictionary<T, UniqueQueueSession> _sessions = new Dictionary<T, UniqueQueueSession>();

        public async Task EnqueueAsync(T item, CancellationToken cancellationtoken = default(CancellationToken))
        {
            TaskCompletionSource<T> tcs = null;

            await _semaphoreSlim.WaitAsync(cancellationtoken);

            if (!_sessions.ContainsKey(item))
            {
                if (_consumerQueue.Count > 0)
                {
                    tcs = _consumerQueue.Dequeue();
                }
                else
                {
                    _producerQueue.Enqueue(item);
                }

                _sessions.Add(item, UniqueQueueSession.Unique);
            }
            else if (_sessions[item] != UniqueQueueSession.RestartAfterRemove)
            {
                _sessions[item] = UniqueQueueSession.RestartAfterRemove;
            }

            _semaphoreSlim.Release();

            if (tcs != null)
            {
                tcs.TrySetResult(item);
            }
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);

            if (_producerQueue.Count > 0)
            {
                var item = _producerQueue.Dequeue();
                _semaphoreSlim.Release();
                return item;
            }
            else
            {
                var tcs = new TaskCompletionSource<T>();
                _consumerQueue.Enqueue(tcs);
                _semaphoreSlim.Release();
                return await tcs.Task;
            }
        }

        public async Task RemoveAsync(T item, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);

            if (_sessions.TryGetValue(item, out var uniqueQueueSession))
            {
                _sessions.Remove(item);
                _semaphoreSlim.Release();

                if (uniqueQueueSession == UniqueQueueSession.RestartAfterRemove)
                {
                    await EnqueueAsync(item, cancellationToken);
                }
            }
            else
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
