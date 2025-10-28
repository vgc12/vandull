using UnityEngine;

namespace DependencyInjection
{
    public class AudioManager : IAudioManager
    {
        
        public void PlaySound(AudioSource audioSource)
        {
            audioSource.Play();
        }
        public float VolumeToDecibels(float volume)
        {
            if (volume <= 0.0001f)
                return -80f; 

            return 20f * Mathf.Log10(volume);
        }
        public float DecibelsToVolume(float dB)
        {
            return Mathf.Pow(10f, dB / 20f);
        }
    }
}