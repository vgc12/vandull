using Attributes;
using UnityEngine;

namespace Items.Guns
{
    public class AudioSettings
    {
        [Required] public AudioSource Shoot;
        [Required] public AudioSource OutOfAmmoClick;
        [Required] public AudioSource Reload;
    }
}