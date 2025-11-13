using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.Lifecycle;

namespace Game.Core.Commands
{
    public sealed class CommandProcessor : IGameService
    {
        private readonly Queue<ICommand> _commandQueue = new();
        private readonly object _lockObject = new();
        private bool _isProcessing = false;
        private CancellationTokenSource _processingCts;

        public int InitializationOrder => 20;

        public void Enqueue(ICommand command)
        {
            lock (_lockObject)
            {
                _commandQueue.Enqueue(command);

                if (!_isProcessing)
                {
                    _ = ProcessQueueAsync();
                }
            }
        }

        private async Task ProcessQueueAsync()
        {
            _isProcessing = true;
            _processingCts = new CancellationTokenSource();

            try
            {
                while (_commandQueue.Count > 0 && !_processingCts.Token.IsCancellationRequested)
                {
                    ICommand command;
                    lock (_lockObject)
                    {
                        if (_commandQueue.Count == 0)
                        {
                            break;
                        }

                        command = _commandQueue.Dequeue();
                    }

                    if (command.CanExecute())
                    {
                        try
                        {
                            await command.ExecuteAsync(_processingCts.Token);
                        }
                        catch (Exception ex)
                        {
                            command.OnExecutionFailed(ex);
                        }
                    }
                }
            }
            finally
            {
                lock (_lockObject)
                {
                    _isProcessing = false;
                }
                _processingCts.Dispose();
                _processingCts = null;
            }
        }

        public Task InitializeAsync(CancellationToken ct) => Task.CompletedTask;

        public async Task ShutdownAsync(CancellationToken ct)
        {
            _processingCts?.Cancel();

            while (_isProcessing)
            {
                await Task.Delay(100, ct);
            }
        }
    }
}
