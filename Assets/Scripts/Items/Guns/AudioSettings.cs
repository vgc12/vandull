using Attributes;
using UnityEngine;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Guns/AudioSettings", order = 1)]
    public class AudioSettings : ScriptableObject
    {
        [Required] public AudioClip shoot;
        [Required] public AudioClip outOfAmmoClick;
        [Required] public AudioClip reload;
    }
}