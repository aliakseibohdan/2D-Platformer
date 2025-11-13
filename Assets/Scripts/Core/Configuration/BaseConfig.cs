using UnityEngine;

namespace Game.Core.Configuration
{
    public abstract class BaseConfig : ScriptableObject
    {
        [field: Header("Base Config Settings")]
        [field: SerializeField]
        public string ConfigId { get; private set; }
        [field: SerializeField]
        public int Version { get; } = 1;
        [field: SerializeField]
        public bool EnableRemoteOverrides { get; } = true;

        public virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(ConfigId))
            {
                ConfigId = $"{GetType().Name}_{System.Guid.NewGuid().ToString("N")[..8]}";
            }
        }

        public virtual void ApplyRemoteOverrides() { }
    }
}
