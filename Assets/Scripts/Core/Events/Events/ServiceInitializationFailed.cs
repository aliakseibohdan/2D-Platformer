using System;

namespace Game.Core.Events.Events
{
    public struct ServiceInitializationFailed : IEvent
    {
        public Type ServiceType { get; set; }
        public Exception Exception { get; set; }
    }
}
