using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items.Guns.Firing
{
    public class EnemyAutomaticFireMode : AutomaticFireMode
    {
        public EnemyAutomaticFireMode(Gun gun, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null) : base(gun,
            onShotFiredSubscribers)
        {
        }

        protected override void PerformRaycast()
        {
            var startPoint = MuzzleTransform.position;
            var horizontalSpread = _gun.recoilSettings.horizontalRecoil;
            var verticalSpread = _gun.recoilSettings.verticalRecoil;


            var spread = Quaternion.Euler(
                Random.Range(-verticalSpread, verticalSpread),
                Random.Range(-horizontalSpread, horizontalSpread),
                0f
            );

            var direction = spread * MuzzleTransform.forward;


            if (Physics.Raycast(startPoint, direction, out var hit, _gun.damageSettings.range,
                    ~LayerMask.GetMask("Ignore Raycast")))
            {
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, hit.point, hit));


                ApplyDamage(hit);
            }
            else
            {
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, direction * _gun.damageSettings.range,
                    new RaycastHit()));
            }
        }
    }
}