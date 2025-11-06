using System;
using System.Collections.Generic;

namespace Items.Guns.Firing
{
    public class AutomaticFireMode : BaseFireMode
    {
        private bool _fireButtonHeld;
        private bool _shouldFire;

        public AutomaticFireMode(Gun gun, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
            : base(gun, onShotFiredSubscribers)
        {
        }

        public override void Fire()
        {
            StartAutomaticFire();
        }


        public override void StopFire()
        {
            _shouldFire = false;
        }

        public override void Update()
        {
            // Should execute this even when out of ammo to make the dry fire sound
            if (_shouldFire) PerformShot();
        }

        private void StartAutomaticFire()
        {
            _shouldFire = true;
        }
    }
}