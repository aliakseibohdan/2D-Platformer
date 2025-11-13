using System.Threading;
using System.Threading.Tasks;

namespace Game.Core.Lifecycle
{
    public interface IGameService
    {
        public Task InitializeAsync(CancellationToken ct);
        public Task ShutdownAsync(CancellationToken ct);
        public int InitializationOrder { get; }
    }
}
