using UnityEngine;
using Game.Core.Configuration;

namespace Game.Gameplay
{
    [CreateAssetMenu(fileName = "GameplayConfig", menuName = "Game/Configs/Gameplay Config")]
    public class GameplayConfig : BaseConfig
    {
        [Header("Player Settings")]
        public float playerMoveSpeed = 5f;
        public float playerRunSpeed = 8f;
        public float playerJumpForce = 7f;
        public float playerGravity = 20f;
        public float playerRotationSpeed = 10f;

        [Header("Camera Settings")]
        public float cameraSensitivity = 2f;
        public bool invertYAxis = false;
        public float cameraFOV = 60f;
        public float cameraZoomSpeed = 5f;
        public float cameraShakeIntensity = 1f;

        [Header("Combat Settings")]
        public float playerHealth = 100f;
        public float playerStamina = 100f;
        public float staminaRegenRate = 10f;
        public float damageMultiplier = 1f;
        public bool enableFriendlyFire = false;

        [Header("Game Rules")]
        public float gameTimeLimit = 300f;
        public int maxPlayers = 4;
        public bool respawnEnabled = true;
        public float respawnTime = 5f;
        public int maxLives = 3;

        [Header("Difficulty Settings")]
        public EDifficultyLevel difficulty = EDifficultyLevel.Normal;
        public float enemyHealthMultiplier = 1f;
        public float enemyDamageMultiplier = 1f;
        public float enemySpawnRateMultiplier = 1f;

        [Header("Physics Settings")]
        public float gravityMultiplier = 1f;
        public float physicsTimeStep = 0.02f;
        public int physicsSolverIterations = 6;

        [Header("Input Settings")]
        public float inputBufferTime = 0.1f;
        public float analogDeadZone = 0.2f;
        public bool enableMouseSmoothing = true;

        public enum EDifficultyLevel
        {
            Easy,
            Normal,
            Hard,
            Expert
        }

        public override void OnValidate()
        {
            base.OnValidate();

            playerMoveSpeed = Mathf.Max(0, playerMoveSpeed);
            playerRunSpeed = Mathf.Max(playerMoveSpeed, playerRunSpeed);
            playerHealth = Mathf.Max(1, playerHealth);
            gameTimeLimit = Mathf.Max(0, gameTimeLimit);
            maxPlayers = Mathf.Clamp(maxPlayers, 1, 16);
            respawnTime = Mathf.Max(0, respawnTime);
            maxLives = Mathf.Max(1, maxLives);
        }

        public float GetEnemyHealthMultiplier() => difficulty switch
        {
            EDifficultyLevel.Easy => enemyHealthMultiplier * 0.7f,
            EDifficultyLevel.Normal => enemyHealthMultiplier,
            EDifficultyLevel.Hard => enemyHealthMultiplier * 1.3f,
            EDifficultyLevel.Expert => enemyHealthMultiplier * 1.8f,
            _ => enemyHealthMultiplier
        };
    }
}
