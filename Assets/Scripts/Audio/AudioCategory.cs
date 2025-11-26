using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    /// <summary>
    ///     Configuration for an audio category with mixer group and spatial settings.
    ///     Configure this in the Inspector to easily separate audio into different mixer groups.
    /// </summary>
    [CreateAssetMenu(fileName = "Audio Category Configuration", menuName = "Audio Category Configuration")]
    public sealed class AudioCategory : ScriptableObject
    {
        [Header("Mixer Group")] [Tooltip("Assign the AudioMixerGroup for this category here")]
        public AudioMixerGroup mixerGroup;

        [Header("Spatial Settings")] [Range(0f, 1f)] [Tooltip("0 = 2D, 1 = 3D")]
        public float spatialBlend = 1f;

        [Tooltip("How sound attenuates over distance")]
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [Tooltip("Distance where sound is at full volume")]
        public float minDistance = 1f;

        [Tooltip("Distance where sound is completely silent")]
        public float maxDistance = 50f;

        [Header("Playback Settings")]
        [Range(0, 256)]
        [Tooltip("Lower priority = more important (0 = highest priority)")]
        public int priority = 128;

        [Range(-3f, 3f)] [Tooltip("Doppler effect intensity")]
        public float dopplerLevel = 1f;

        [Range(0f, 360f)] [Tooltip("Stereo spread angle")]
        public float spread;

        /// <summary>
        ///     Applies this category's configuration to an AudioSource.
        /// </summary>
        public void ApplyTo(AudioSource audioSource)
        {
            audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.spatialBlend = spatialBlend;
            audioSource.rolloffMode = rolloffMode;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.priority = priority;
            audioSource.dopplerLevel = dopplerLevel;
            audioSource.spread = spread;
        }
    }
}