using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Guns.Firing
{
    public class BurstFireMode : BaseFireMode
    {
        private readonly int _burstCount;
        private readonly float _burstDelay;
        private Coroutine _burstFireCoroutine;

        public BurstFireMode(Gun gun, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
            : base(gun, onShotFiredSubscribers)
        {
            _burstCount = Gun.firingSettings.burstCount;
            _burstDelay = Gun.firingSettings.burstDelay;
        }


        public override void Fire()
        {
            StartBurstFire();
        }


        public override void StopFire()
        {
        }

        private void StartBurstFire()
        {
            if (_burstFireCoroutine != null) return;

            _burstFireCoroutine = Behaviour.StartCoroutine(FireBurst());
        }

        private void StopBurstFire()
        {
            if (_burstFireCoroutine == null) return;
            Behaviour.StopCoroutine(_burstFireCoroutine);
            _burstFireCoroutine = null;
        }

        private IEnumerator FireBurst()
        {
            for (var i = 0; i < _burstCount; i++)
            {
                PerformShot();


                if (i < _burstCount - 1) yield return new WaitForSeconds(_burstDelay);
            }

            yield return new WaitForSeconds(Gun.firingSettings.fireRate - _burstDelay);
            _burstFireCoroutine = null;
        }
    }
}