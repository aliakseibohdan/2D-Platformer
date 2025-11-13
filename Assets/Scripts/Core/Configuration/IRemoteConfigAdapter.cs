using System.Threading;
using System.Threading.Tasks;

namespace Game.Core.Configuration
{
    public interface IRemoteConfigAdapter
    {
        public Task ApplyOverridesAsync(CancellationToken ct);
        public bool SupportsHotSwap { get; }
    }
}
