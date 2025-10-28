using Attributes;
using UnityEngine;
using UnityEngine.Audio;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Guns/AudioSettings", order = 1)]
    public class AudioSettings : ScriptableObject
    {
        [Required] public AudioClip shoot;
        [Required] public AudioClip outOfAmmoClick;
        [Required] public AudioClip reload;
        [Required]   public AudioMixerGroup audioMixerGroup;
        [Required]public Vector2 pitchRange;
    }
}