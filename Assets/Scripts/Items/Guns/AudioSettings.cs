using System;
using Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "Guns/AudioSettings", order = 1)]
    public sealed class AudioSettings : ScriptableObject
    {
        public GunAudioClip fire;
        public GunAudioClip dryFire;
        public GunAudioClip boltPullBack;
        public GunAudioClip boltRelease;
        public GunAudioClip magRemoved;
        public GunAudioClip magInserted;
        public GunAudioClip equip;
        public GunAudioClip bodyPartHit;
        public GunAudioClip headPartHit;


        [Serializable]
        public class GunAudioClip
        {
            [Required] public AudioClip clip;
            public Vector2 pitchRange = new(1f, 1f);
            public float spatialBlend = 1f;

            public float RandomPitch => Random.Range(
                pitchRange.x,
                pitchRange.y
            );
        }
    }
}