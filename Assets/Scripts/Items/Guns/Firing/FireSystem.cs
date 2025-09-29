using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using EventBus;
using General;
using Items.Guns.Firing;
using Items.Guns.Items.Guns;
using NPC;
using Player;

namespace Items.Guns
{


    public abstract class BaseFireMode : IFireSystem
    {
        protected readonly GunConfig Config;
        public  Transform MuzzleTransform { get; }
        protected readonly Transform Transform;
        protected readonly MonoBehaviour Behaviour;
        protected readonly RaycastHit[] HitResults = new RaycastHit[10];

        protected float LastFireTime;
        protected bool IsOutOfAmmo;



        public abstract void ExecuteFireCommand(FireCommand command);

        public virtual bool CanFire => Time.time > LastFireTime + Config.firingSettings.fireRate && !IsOutOfAmmo;

        public event Action<ShotFiredEvent> OnShotFired;

        protected BaseFireMode(GunConfig config, Transform gunTransform, MonoBehaviour behaviour, Transform muzzleTransform,
            List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Transform = gunTransform ?? throw new ArgumentNullException(nameof(gunTransform));
            Behaviour = behaviour ?? throw new ArgumentNullException(nameof(behaviour));

            MuzzleTransform = muzzleTransform ?? throw new ArgumentNullException(nameof(muzzleTransform));

            if (onShotFiredSubscribers == null) return;
            foreach (var subscriber in onShotFiredSubscribers)
            {
                OnShotFired += subscriber;
            }

        }

        public abstract void Fire();




        public abstract void StopFire();

        public virtual void Update()
        {
        }

        public void OnOutOfAmmo()
        {
            IsOutOfAmmo = true;
        }

        public void OnReloadEnded()
        {
            IsOutOfAmmo = false;
        }

        protected void PerformShot()
        {
            if (!CanFire) return;

            LastFireTime = Time.time;
            PerformRaycast();
        }

        private void PerformRaycast()
        {
            var startPoint = MuzzleTransform.position;
            var endPoint = startPoint + (MuzzleTransform.forward * Config.damageSettings.range);

            var hitCount = Physics.RaycastNonAlloc(
                startPoint,
                MuzzleTransform.forward,
                HitResults,
                Config.damageSettings.range,
                ~LayerMask.GetMask("Ignore Raycast"));

            if (hitCount <= 0)
            {
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, endPoint, new RaycastHit()));
                return;
            }

            ProcessHits(hitCount, startPoint);
        }

        private void ProcessHits(int hitCount, Vector3 startPoint)
        {
            for (var i = 0; i < hitCount; i++)
            {
                var hit = HitResults[i];

                if (hit.collider == null) continue;
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, hit.point, hit));

//                VandullLogger.Log("Hit: " + hit.collider.name);
                
                
                if (!hit.collider.transform.root.TryGetComponent<IDamageable>(out var damageable) &&
                    !hit.collider.transform.root.TryGetComponent(out damageable)) continue;
                if (hit.collider.TryGetComponent<BodyPart>(out var bodyPart))
                {
                    damageable.TakeDamage(Config.damageSettings.damage * bodyPart.damageMultiplier, MuzzleTransform.forward);
                    EventBus<GunFiredEvent>.Raise(new GunFiredEvent(Transform.position,
                        Config.damageSettings.damage));
                }
                else
                {
                    damageable.TakeDamage(Config.damageSettings.damage, MuzzleTransform.forward);
                }
            }
        }
    }

}
