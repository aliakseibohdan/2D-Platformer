using Game.Core.Lifecycle;

namespace Game.Core.Events.Events
{
    public struct GameStateChanged : IEvent
    {
        public EGameState PreviousState { get; set; }
        public EGameState NewState { get; set; }
    }
}
