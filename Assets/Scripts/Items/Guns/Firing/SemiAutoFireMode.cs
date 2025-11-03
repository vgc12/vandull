using System;
using System.Collections.Generic;

namespace Items.Guns.Firing
{
    public class SemiAutoFireMode : BaseFireMode
    {
        public SemiAutoFireMode(Gun gun, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
            : base(gun, onShotFiredSubscribers)
        {
        }


        public override void Fire()
        {
            PerformShot();
        }


        public override void StopFire()
        {
            // Noop
        }
    }
}