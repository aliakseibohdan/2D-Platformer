using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Core.Configuration;
using Game.Core.Events;
using Game.Core.Events.Events;
using Game.Core.Services;
using UnityEngine;

namespace Game.Core.Lifecycle
{
    public sealed class LifecycleManager : MonoBehaviour
    {
        [SerializeField] private EGameState _initialState = EGameState.Boot;
        [SerializeField] private List<MonoBehaviour> _servicePrefabs = new();
        private ConfigManager _configManager;
        private List<IGameService> _services = new();
        private CancellationTokenSource _shutdownCts;

        public event Action<EGameState> onStateChanged;

        public EGameState CurrentState { get; private set; }
        public ServiceLocator ServiceLocator { get; private set; }
        public EventBus EventBus { get; private set; }

        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _shutdownCts = new CancellationTokenSource();

            InitializeCoreSystems();
            await InitializeServicesAsync();

            TransitionTo(EGameState.MainMenu);
        }

        private async void OnApplicationQuit() => await ShutdownAsync();

        private void InitializeCoreSystems()
        {
            ServiceLocator = new ServiceLocator();
            EventBus = new EventBus();
            _configManager = new ConfigManager();

            ServiceLocator.Register<IConfigProvider>(_configManager);
            ServiceLocator.Register(EventBus);

            ServiceLocator.LockRegistration();
        }

        private async Task InitializeServicesAsync()
        {
            foreach (var prefab in _servicePrefabs)
            {
                if (prefab is IGameService service)
                {
                    var serviceInstance = Instantiate(prefab);
                    DontDestroyOnLoad(serviceInstance.gameObject);

                    _services.Add(serviceInstance as IGameService);
                    RegisterServiceInterfaces(serviceInstance);
                }
            }

            _services = _services.OrderBy(s => s.InitializationOrder).ToList();

            foreach (var service in _services)
            {
                try
                {
                    await service.InitializeAsync(_shutdownCts.Token);
                    Debug.Log($"Successfully initialized service: {service.GetType().Name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to initialize service {service.GetType().Name}: {ex}");
                    HandleServiceInitializationFailure(service, ex);
                    throw;
                }
            }

            await _configManager.InitializeAsync(_shutdownCts.Token);
        }

        private void RegisterServiceInterfaces(MonoBehaviour serviceInstance)
        {
            var interfaces = serviceInstance.GetType().GetInterfaces()
                .Where(i => i != typeof(IGameService) && i != typeof(IDisposable));

            foreach (var interfaceType in interfaces)
            {
                var method = typeof(ServiceLocator).GetMethod("Register")?.MakeGenericMethod(interfaceType);
                _ = (method?.Invoke(ServiceLocator, new[] { serviceInstance }));
            }
        }

        private void HandleServiceInitializationFailure(IGameService failedService, Exception ex)
        {
            EventBus.Publish(new ServiceInitializationFailed
            {
                ServiceType = failedService.GetType(),
                Exception = ex
            });

            Debug.LogError($"Critical service {failedService.GetType().Name} failed to initialize: {ex}");

            TransitionTo(EGameState.MainMenu);
        }

        public void TransitionTo(EGameState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            var previousState = CurrentState;
            CurrentState = newState;

            Debug.Log($"Game state transition: {previousState} -> {newState}");

            onStateChanged?.Invoke(newState);
            EventBus.Publish(new GameStateChanged
            {
                PreviousState = previousState,
                NewState = newState
            });

            HandleStateTransition(newState);
        }

        private void HandleStateTransition(EGameState toState)
        {
            switch (toState)
            {
                case EGameState.Boot:
                    break;
                case EGameState.Preload:
                    break;
                case EGameState.MainMenu:
                    break;
                case EGameState.Loading:
                    break;
                case EGameState.Gameplay:
                    Time.timeScale = 1f;
                    break;
                case EGameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case EGameState.Shutdown:
                    _ = ShutdownAsync();
                    break;
                default:
                    break;
            }
        }

        public void Suspend()
        {
            if (CurrentState == EGameState.Gameplay)
            {
                TransitionTo(EGameState.Paused);
            }
        }

        public void Resume()
        {
            if (CurrentState == EGameState.Paused)
            {
                TransitionTo(EGameState.Gameplay);
            }
        }

        public async Task ShutdownAsync()
        {
            if (CurrentState == EGameState.Shutdown)
            {
                return;
            }

            TransitionTo(EGameState.Shutdown);
            _shutdownCts.Cancel();

            for (int i = _services.Count - 1; i >= 0; i--)
            {
                try
                {
                    await _services[i].ShutdownAsync(_shutdownCts.Token);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error shutting down service {_services[i].GetType().Name}: {ex}");
                }
            }

            ServiceLocator?.Dispose();
            EventBus?.Dispose();
            _shutdownCts?.Dispose();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
