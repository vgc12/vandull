using System;
using General;
using UnityEngine;

namespace Items.Guns.Trail
{
    [CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Trail Config", order = 4)]
    public class TrailConfig : ScriptableObject, ICloneable
    {
        public Material material;
        public AnimationCurve widthCurve;
        public float duration = 0.5f;
        public float minVertexDistance = 0.1f;
        public Gradient color;

        public float missDistance = 100f;
        public float simulationSpeed = 100f;

        public object Clone()
        {
            var config = CreateInstance<TrailConfig>();

            Utilities.CopyValues(this, config);

            return config;
        }
    }
}