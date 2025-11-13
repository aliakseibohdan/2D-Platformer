using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.Lifecycle;
using UnityEngine;

namespace Game.Core.Configuration
{
    public sealed class ConfigManager : IConfigProvider, IGameService
    {
        private readonly Dictionary<Type, ScriptableObject> _configs = new();
        private readonly List<IRemoteConfigAdapter> _remoteAdapters = new();
        private bool _initialized = false;

        public int InitializationOrder => 10;

        public T GetConfig<T>() where T : ScriptableObject
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("ConfigManager not initialized");
            }

            var configType = typeof(T);
            if (_configs.TryGetValue(configType, out var config))
            {
                return (T)config;
            }

            throw new KeyNotFoundException($"Config of type {configType.Name} not found");
        }

        public bool TryGetConfig<T>(out T config) where T : ScriptableObject
        {
            config = default;

            if (!_initialized || !_configs.TryGetValue(typeof(T), out var rawConfig))
            {
                return false;
            }

            config = (T)rawConfig;
            return true;
        }

        public void RegisterConfig(ScriptableObject config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var configType = config.GetType();
            if (_configs.ContainsKey(configType))
            {
                throw new InvalidOperationException($"Config of type {configType.Name} already registered");
            }

            _configs[configType] = config;
        }

        public void RegisterRemoteAdapter(IRemoteConfigAdapter adapter) => _remoteAdapters.Add(adapter);

        public async Task ApplyRemoteOverridesAsync(CancellationToken ct)
        {
            if (_remoteAdapters.Count == 0)
            {
                return;
            }

            var tasks = _remoteAdapters.Select(adapter => adapter.ApplyOverridesAsync(ct));
            await Task.WhenAll((IEnumerable<Task>)tasks);
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            if (_initialized)
            {
                return;
            }

            var configs = Resources.LoadAll<ScriptableObject>("Configs");
            foreach (var config in configs)
            {
                RegisterConfig(config);
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            await ApplyRemoteOverridesAsync(ct);
#endif

            _initialized = true;
        }

        public Task ShutdownAsync(CancellationToken ct)
        {
            _configs.Clear();
            _remoteAdapters.Clear();
            _initialized = false;
            return Task.CompletedTask;
        }
    }
}
