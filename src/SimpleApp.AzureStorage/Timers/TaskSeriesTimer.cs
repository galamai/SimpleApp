using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleApp.AzureStorage.Timers
{
    internal sealed class TaskSeriesTimer : IDisposable
    {
        private readonly Func<CancellationToken, Task<TaskSeriesResult>> _command;
        private readonly ILogger _logger;
        private readonly Task _initialWait;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task _run;
        private bool _stopped;
        private bool _disposed;

        public static TaskSeriesTimer StartNew(Func<CancellationToken, Task<TaskSeriesResult>> command, ILogger logger, Task initialWait = null)
        {
            var timer = new TaskSeriesTimer(command, logger, initialWait);
            timer.Start();
            return timer;
        }

        private TaskSeriesTimer(Func<CancellationToken, Task<TaskSeriesResult>> command, ILogger logger, Task initialWait)
        {
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _initialWait = initialWait ?? Task.CompletedTask;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (!_stopped)
            {
                return CoreWaitAsync(cancellationToken);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (!_stopped)
            {
                _cancellationTokenSource.Cancel();
                return CoreStopAsync(cancellationToken);
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _disposed = true;
            }
        }

        private void Start()
        {
            _run = RunAsync(_cancellationTokenSource.Token);
        }

        private async Task CoreWaitAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            var cancellationTaskSource = new TaskCompletionSource<object>();

            using (cancellationToken.Register(() => cancellationTaskSource.SetCanceled()))
            {
                await Task.WhenAny(_run, cancellationTaskSource.Task).ConfigureAwait(false);
            }
        }

        private async Task CoreStopAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();

            var cancellationTaskSource = new TaskCompletionSource<object>();

            using (cancellationToken.Register(() => cancellationTaskSource.SetCanceled()))
            {
                await Task.WhenAny(_run, cancellationTaskSource.Task).ConfigureAwait(false);
            }
            _stopped = true;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Yield();

                Task wait = _initialWait;

                while (!cancellationToken.IsCancellationRequested)
                {
                    var cancellationTaskSource = new TaskCompletionSource<object>();

                    using (cancellationToken.Register(() => cancellationTaskSource.SetCanceled()))
                    {
                        try
                        {
                            await Task.WhenAny(wait, cancellationTaskSource.Task).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            // It`s ok;
                        }
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        var commandResult = await _command.Invoke(cancellationToken).ConfigureAwait(false);
                        wait = commandResult.Wait;
                    }
                    catch (OperationCanceledException)
                    {
                        // It`s ok;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(new EventId(), ex, ex.Message);
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(null);
            }
        }
    }
}
