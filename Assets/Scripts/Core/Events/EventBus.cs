using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Core.Events
{
    public sealed class EventBus : IDisposable
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly CancellationTokenSource _cts = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            _lock.EnterWriteLock();
            try
            {
                var eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<Delegate>();
                }

                _handlers[eventType].Add(handler);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            _lock.EnterWriteLock();
            try
            {
                var eventType = typeof(TEvent);
                if (_handlers.ContainsKey(eventType))
                {
                    _ = _handlers[eventType].Remove(handler);
                    if (_handlers[eventType].Count == 0)
                    {
                        _ = _handlers.Remove(eventType);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : IEvent
        {
            List<Delegate> handlers;
            _lock.EnterReadLock();
            try
            {
                var eventType = typeof(TEvent);
                if (!_handlers.TryGetValue(eventType, out handlers) || handlers.Count == 0)
                {
                    return;
                }

                handlers = new List<Delegate>(handlers);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (UnityEngine.Application.isPlaying)
            {
                Dispatcher.ExecuteOnMainThread(() =>
                {
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            ((Action<TEvent>)handler)(eventData);
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogError($"Error executing event handler for {typeof(TEvent).Name}: {ex}");
                        }
                    }
                });
            }
            else
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        ((Action<TEvent>)handler)(eventData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Error executing event handler for {typeof(TEvent).Name}: {ex}");
                    }
                }
            }
        }

        public async Task PublishAsync<TEvent>(TEvent eventData, CancellationToken ct) where TEvent : IEvent
        {
            var linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token).Token;

            await Task.Run(() =>
            {
                List<Delegate> handlers;
                _lock.EnterReadLock();
                try
                {
                    var eventType = typeof(TEvent);
                    if (!_handlers.TryGetValue(eventType, out handlers) || handlers.Count == 0)
                    {
                        return;
                    }

                    handlers = new List<Delegate>(handlers);
                }
                finally
                {
                    _lock.ExitReadLock();
                }

                ParallelOptions parallelOptions = new()
                {
                    CancellationToken = linkedCt,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                try
                {
                    _ = Parallel.ForEach(handlers, parallelOptions, handler =>
                    {
                        linkedCt.ThrowIfCancellationRequested();
                        try
                        {
                            ((Action<TEvent>)handler)(eventData);
                        }
                        catch (Exception ex) when (ex is not OperationCanceledException)
                        {
                            UnityEngine.Debug.LogError($"Error executing async event handler for {typeof(TEvent).Name}: {ex}");
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                }
            }, linkedCt);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _lock?.Dispose();
        }
    }
}
