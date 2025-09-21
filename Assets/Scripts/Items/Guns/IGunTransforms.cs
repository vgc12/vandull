using UnityEngine;

namespace Items.Guns.Items.Guns.Dependencies
{
    public interface IGunTransforms
    {
        Transform GunTransform { get; }

        Transform HipFireTransform { get; }
        Transform AdsTransform { get; }
        Transform RecoilTransform { get; }
    }
}