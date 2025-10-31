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
            _burstCount = _gun.firingSettings.burstCount;
            _burstDelay = _gun.firingSettings.burstDelay;
        }


        public override void ExecuteFireCommand(FireCommand command)
        {
            switch (command)
            {
                case FireCommand.SingleShot:
                    StartBurstFire();
                    break;
                case FireCommand.StartAutomaticFire:
                    break;
                case FireCommand.StopAutomaticFire:
                    StopFire();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }
        }

        public override void Fire()
        {
        }


        public override void StopFire()
        {
            StopBurstFire();
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

            yield return new WaitForSeconds(_gun.firingSettings.fireRate - _burstDelay);
            _burstFireCoroutine = null;
        }
    }
}