using UnityEngine;
using Game.Core.Configuration;

namespace Game.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Game/Configs/Audio Config")]
    public class AudioConfig : BaseConfig
    {
        [Header("Volume Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.8f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float voiceVolume = 0.8f;
        [Range(0f, 1f)] public float ambientVolume = 0.6f;

        [Header("Audio Mixer Settings")]
        public string masterVolumeParameter = "MasterVolume";
        public string musicVolumeParameter = "MusicVolume";
        public string sfxVolumeParameter = "SFXVolume";
        public string voiceVolumeParameter = "VoiceVolume";
        public string ambientVolumeParameter = "AmbientVolume";

        [Header("Music Settings")]
        public float musicFadeInDuration = 2f;
        public float musicFadeOutDuration = 1.5f;
        public bool loopBackgroundMusic = true;

        [Header("SFX Settings")]
        public int maxConcurrentSFX = 20;
        public float sfxPoolWarmupCount = 10;
        public float spatialBlend = 0f;

        [Header("Advanced Settings")]
        public bool enableAudioOcclusion = true;
        public float occlusionUpdateFrequency = 0.1f;
        public bool enableDopplerEffect = true;
        public float dopplerLevel = 1f;

        [Header("Platform Specific")]
        public float mobileMasterVolumeMultiplier = 0.8f;
        public bool mobileReduceQuality = true;

        public override void OnValidate()
        {
            base.OnValidate();

            masterVolume = Mathf.Clamp01(masterVolume);
            musicVolume = Mathf.Clamp01(musicVolume);
            sfxVolume = Mathf.Clamp01(sfxVolume);
            voiceVolume = Mathf.Clamp01(voiceVolume);
            ambientVolume = Mathf.Clamp01(ambientVolume);

            maxConcurrentSFX = Mathf.Max(1, maxConcurrentSFX);
            sfxPoolWarmupCount = Mathf.Max(0, sfxPoolWarmupCount);
        }
    }
}
