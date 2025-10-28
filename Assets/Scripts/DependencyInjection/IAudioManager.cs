using UnityEngine;

namespace DependencyInjection
{
    public interface IAudioManager
    {
        void PlaySound(AudioSource audioSource);

        float VolumeToDecibels(float volume);

        float DecibelsToVolume(float dB);
    }
}