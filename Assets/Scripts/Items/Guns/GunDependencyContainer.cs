using JetBrains.Annotations;
using UnityEngine;

namespace Items.Guns.Items.Guns.Dependencies
{
    
    public class GunDependencyContainer : IGunTransforms, IGunConfiguration, IMonoBehaviourContext
    {
        public Transform GunTransform { get; private set; }
        [CanBeNull] public Transform HipFireTransform { get; private set; }
        [CanBeNull] public Transform AdsTransform { get; private set; }
        [CanBeNull] public Transform RecoilTransform { get; private set; }
        public GunConfig Config { get; private set; }
        public MonoBehaviour Behaviour { get; private set; }

        public GunDependencyContainer(
            Transform gunTransform,
          
            GunConfig config,
            MonoBehaviour behaviour,
            [CanBeNull] Transform hipFireTransform = null,
            [CanBeNull] Transform adsTransform = null,
            [CanBeNull] Transform recoilTransform = null)
        {
            GunTransform = gunTransform;
           
            HipFireTransform = hipFireTransform;
            AdsTransform = adsTransform;
            RecoilTransform = recoilTransform;
            Config = config;
            Behaviour = behaviour;
        }
    }
}