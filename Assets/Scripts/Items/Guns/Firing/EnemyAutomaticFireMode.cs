using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items.Guns.Firing
{
    public sealed class EnemyAutomaticFireMode : AutomaticFireMode
    {
        public EnemyAutomaticFireMode(Gun gun, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null) : base(gun,
            onShotFiredSubscribers)
        {
        }

        protected override void PerformRaycasts()
        {
            var startPoint = MuzzleTransform.position;
            var horizontalSpread = Gun.recoilSettings.horizontalRecoil;
            var verticalSpread = Gun.recoilSettings.verticalRecoil;


            var spread = Quaternion.Euler(
                Random.Range(-verticalSpread, verticalSpread),
                Random.Range(-horizontalSpread, horizontalSpread),
                0f
            );

            var direction = spread * MuzzleTransform.forward;


            if (Physics.Raycast(startPoint, direction, out var hit, Gun.damageSettings.range,
                    ~LayerMask.GetMask("Ignore Raycast")))
            {
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, Gun.trailStartPoint.position, hit.point, hit));


                ApplyDamage(hit);
            }
            else
            {
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, Gun.trailStartPoint.position,
                    direction * Gun.damageSettings.range,
                    new RaycastHit()));
            }
        }
    }
}