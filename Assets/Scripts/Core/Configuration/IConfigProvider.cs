using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Core.Configuration
{
    public interface IConfigProvider
    {
        public T GetConfig<T>() where T : ScriptableObject;
        public bool TryGetConfig<T>(out T config) where T : ScriptableObject;
        public void RegisterConfig(ScriptableObject config);
        public Task ApplyRemoteOverridesAsync(CancellationToken ct);
    }
}
