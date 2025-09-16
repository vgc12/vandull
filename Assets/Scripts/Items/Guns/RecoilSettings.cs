using System;
using General;
using UnityEngine;

namespace Items.Guns
{
  [CreateAssetMenu (fileName = "Recoil Settings", menuName = "Guns/Recoil Settings", order = 4)]
    public class RecoilSettings : ScriptableObject , ICloneable
    {
        [Header("Visual Recoil (Camera)")]
        public float verticalRecoil = 2f;
        public float horizontalRecoil = 1f;
        public float recoilSpeed = 15f;
        public float returnSpeed = 8f;
        public AnimationCurve recoilPattern = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Physical Recoil (Gun Position)")]
        public Vector3 positionRecoil = new Vector3(0, 0, -0.1f);
        public Vector3 rotationRecoil = new Vector3(-5f, 0, 0);
        public float physicalRecoilSpeed = 20f;
        public float physicalReturnSpeed = 10f;
        
        [Header("Progressive Recoil")]
        public bool useProgressiveRecoil = true;
        public float recoilMultiplierPerShot = 1.2f;
        public float maxRecoilMultiplier = 3f;
        public float recoilDecayRate = 2f;
        public float recoilEffectMultiplier;
        public object Clone()
        {
            RecoilSettings config = CreateInstance<RecoilSettings>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}