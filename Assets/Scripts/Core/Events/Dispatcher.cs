using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Events
{
    public static class Dispatcher
    {
        private static readonly Queue<Action> _executionQueue = new();
        private static readonly object _lockObject = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            GameObject dispatcherHost = new("DispatcherHost");
            _ = dispatcherHost.AddComponent<DispatcherMonoBehaviour>();
            UnityEngine.Object.DontDestroyOnLoad(dispatcherHost);
        }

        public static void ExecuteOnMainThread(Action action)
        {
            lock (_lockObject)
            {
                _executionQueue.Enqueue(action);
            }
        }

        private class DispatcherMonoBehaviour : MonoBehaviour
        {
            private void Update()
            {
                lock (_lockObject)
                {
                    while (_executionQueue.Count > 0)
                    {
                        var action = _executionQueue.Dequeue();
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error in dispatched action: {ex}");
                        }
                    }
                }
            }
        }
    }
}
