using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Game.Core.Services
{
    public sealed class ServiceLocator : IDisposable
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly ReaderWriterLockSlim _lock = new();
        private bool _registrationLocked = false;

        public void Register<TInterface>(TInterface implementation) where TInterface : class
        {
            if (_registrationLocked)
            {
                throw new InvalidOperationException("Service registration is locked after boot phase");
            }

            _lock.EnterWriteLock();
            try
            {
                var serviceType = typeof(TInterface);
                if (_services.ContainsKey(serviceType))
                {
                    throw new InvalidOperationException($"Service {serviceType.Name} is already registered");
                }

                _services[serviceType] = implementation ?? throw new ArgumentNullException(nameof(implementation));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public TInterface Resolve<TInterface>() where TInterface : class
        {
            _lock.EnterReadLock();
            try
            {
                var serviceType = typeof(TInterface);
                if (_services.TryGetValue(serviceType, out object service))
                {
                    return (TInterface)service;
                }

                throw new InvalidOperationException($"Service {serviceType.Name} is not registered");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool TryResolve<TInterface>(out TInterface service) where TInterface : class
        {
            _lock.EnterReadLock();
            try
            {
                var serviceType = typeof(TInterface);
                if (_services.TryGetValue(serviceType, out object svc))
                {
                    service = (TInterface)svc;
                    return true;
                }

                service = default;
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IReadOnlyList<object> GetAllServices()
        {
            _lock.EnterReadLock();
            try
            {
                return _services.Values.ToList().AsReadOnly();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void LockRegistration() => _registrationLocked = true;

        public void Dispose() => _lock?.Dispose();
    }
}
