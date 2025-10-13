using System;
using System.Collections.Generic;
using General;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items.Guns.Firing
{
    public class EnemyAutomaticFireMode : AutomaticFireMode
    {
        public EnemyAutomaticFireMode(GunConfig config, Transform gunTransform, MonoBehaviour behaviour,
            Transform muzzleTransform, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null) : base(config,
            gunTransform, behaviour, muzzleTransform, onShotFiredSubscribers)
        {
        }

        protected override void PerformRaycast()
        {
            var startPoint = MuzzleTransform.position;
            var horizontalSpread = Config.recoilSettings.horizontalRecoil;
            var verticalSpread = Config.recoilSettings.verticalRecoil;


            var spread = Quaternion.Euler(
                Random.Range(-verticalSpread, verticalSpread),
                Random.Range(-horizontalSpread, horizontalSpread),
                0f
            );

            Vector3 direction = spread * MuzzleTransform.forward;

         

            if (Physics.Raycast(startPoint, direction, out var hit, Config.damageSettings.range,
                    ~LayerMask.GetMask("Ignore Raycast")))
            {
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, hit.point, hit));

                VandullLogger.Log("Hit: " + hit.collider.name);

                ApplyDamage(hit);
            }
            else
            {
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, direction * Config.damageSettings.range, new RaycastHit()));
            }
        }
    }
}