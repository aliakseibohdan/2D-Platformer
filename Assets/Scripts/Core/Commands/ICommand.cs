using System;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Core.Commands
{
    public interface ICommand
    {
        public Task ExecuteAsync(CancellationToken ct);
        public bool CanExecute();
        public void OnExecutionFailed(Exception ex);
    }
}
