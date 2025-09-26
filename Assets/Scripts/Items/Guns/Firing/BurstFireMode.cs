using System;
using System.Collections;
using System.Collections.Generic;
using Items.Guns.Firing;
using UnityEngine;

namespace Items.Guns
{
    public class BurstFireMode : BaseFireMode
    {
        private Coroutine _burstFireCoroutine;
        private readonly int _burstCount;
        private readonly float _burstDelay;

        public BurstFireMode(GunConfig config, Transform gunTransform, MonoBehaviour behaviour, Transform muzzleTransform, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
            : base(config, gunTransform, behaviour,muzzleTransform, onShotFiredSubscribers)
        {
            _burstCount = Config.firingSettings.burstCount;
            _burstDelay = Config.firingSettings.burstDelay;
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
            if (!CanFire) return;

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
            for (int i = 0; i < _burstCount && !IsOutOfAmmo; i++)
            {
                if (CanFire)
                {
                    PerformShot();
                }

                if (i < _burstCount - 1)
                {
                    yield return new WaitForSeconds(_burstDelay);
                }
            }

            yield return new WaitForSeconds(Config.firingSettings.fireRate - _burstDelay);
            _burstFireCoroutine = null;
        }
    }
}