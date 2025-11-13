using UnityEngine;
using Game.Core.Configuration;

namespace Game.Graphics
{
    [CreateAssetMenu(fileName = "GraphicsConfig", menuName = "Game/Configs/Graphics Config")]
    public class GraphicsConfig : BaseConfig
    {
        [Header("Quality Settings")]
        public int targetFrameRate = 60;
        public EVSyncMode vSyncMode = EVSyncMode.EveryVBlank;
        public int antiAliasing = 4;
        public bool enablePostProcessing = true;
        public bool enableBloom = true;
        public bool enableMotionBlur = false;
        public bool enableDepthOfField = true;

        [Header("Resolution Settings")]
        public bool fullscreen = true;
        public EResolutionType resolutionType = EResolutionType.Native;
        public Vector2Int customResolution = new(1920, 1080);

        [Header("Shadow Settings")]
        public EShadowQuality shadowQuality = EShadowQuality.HardAndSoft;
        public EShadowResolution shadowResolution = EShadowResolution.High;
        public float shadowDistance = 50f;

        [Header("Texture Settings")]
        public ETextureQuality textureQuality = ETextureQuality.High;
        public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Enable;
        public bool enableRealtimeReflections = true;

        [Header("Performance Settings")]
        public ELODBiasMode lodBiasMode = ELODBiasMode.Balanced;
        public bool enableOcclusionCulling = true;
        public float drawDistance = 1000f;

        [Header("Mobile Specific")]
        public bool mobileReduceShadows = true;
        public bool mobileReduceParticles = false;
        public int mobileTargetFrameRate = 30;

        public enum EVSyncMode
        {
            DontSync = 0,
            EveryVBlank = 1,
            EverySecondVBlank = 2
        }

        public enum EResolutionType
        {
            Native,
            Custom
        }

        public enum EShadowQuality
        {
            Disabled,
            HardOnly,
            HardAndSoft
        }

        public enum EShadowResolution
        {
            Low = 512,
            Medium = 1024,
            High = 2048,
            Ultra = 4096
        }

        public enum ETextureQuality
        {
            VeryLow = 0,
            Low = 1,
            Medium = 2,
            High = 3,
            VeryHigh = 4
        }

        public enum ELODBiasMode
        {
            Performance,
            Balanced,
            Quality
        }

        public override void OnValidate()
        {
            base.OnValidate();

            targetFrameRate = Mathf.Clamp(targetFrameRate, 30, 240);
            antiAliasing = Mathf.Clamp(antiAliasing, 0, 8);
            shadowDistance = Mathf.Max(0, shadowDistance);
            drawDistance = Mathf.Max(100, drawDistance);
        }
    }
}
