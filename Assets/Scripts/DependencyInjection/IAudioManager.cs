using UnityEngine;

namespace DependencyInjection
{
    public interface IAudioManager
    {
        void PlaySound(AudioSource audioSource);
    }
}