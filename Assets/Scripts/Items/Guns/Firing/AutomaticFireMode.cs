using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Guns.Firing
{
    public class AutomaticFireMode : BaseFireMode
    {
        private Coroutine _autoFireCoroutine;
        private bool _fireButtonHeld;

        public AutomaticFireMode(Gun gun, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
            : base(gun, onShotFiredSubscribers)
        {
        }


        public override void ExecuteFireCommand(FireCommand command)
        {
            switch (command)
            {
                case FireCommand.SingleShot:
                    break;
                case FireCommand.StartAutomaticFire:
                    StartAutomaticFire();
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
            if (_autoFireCoroutine == null) return;
            Behaviour.StopCoroutine(_autoFireCoroutine);
            _autoFireCoroutine = null;
        }

        public override void Update()
        {
        }

        private void StartAutomaticFire()
        {
            if (_autoFireCoroutine != null) return;
            if (!CanFire) return;

            _autoFireCoroutine = Behaviour.StartCoroutine(AutomaticFireRoutine());
        }


        private IEnumerator AutomaticFireRoutine()
        {
            while (!IsOutOfAmmo)
            {
                if (CanFire) PerformShot();

                yield return new WaitForSeconds(_gun.firingSettings.fireRate);
            }

            _autoFireCoroutine = null;
        }
    }
}