using System;
using System.Collections.Generic;
using EventBus;
using General;
using UnityEngine;

namespace Items.Guns.Firing
{
    public abstract class BaseFireMode : IFireSystem
    {
        protected readonly MonoBehaviour Behaviour;
        protected readonly GunConfig Config;
        protected readonly RaycastHit[] HitResults = new RaycastHit[10];
        protected readonly Transform Transform;
        protected bool IsOutOfAmmo;

        protected float LastFireTime;

        protected BaseFireMode(GunConfig config, Transform gunTransform, MonoBehaviour behaviour,
            Transform muzzleTransform,
            List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Transform = gunTransform ?? throw new ArgumentNullException(nameof(gunTransform));
            Behaviour = behaviour ?? throw new ArgumentNullException(nameof(behaviour));

            MuzzleTransform = muzzleTransform ?? throw new ArgumentNullException(nameof(muzzleTransform));

            if (onShotFiredSubscribers == null) return;
            foreach (var subscriber in onShotFiredSubscribers) OnShotFired += subscriber;
        }

        public Transform MuzzleTransform { get; }


        public abstract void ExecuteFireCommand(FireCommand command);

        public virtual bool CanFire => Time.time > LastFireTime + Config.firingSettings.fireRate && !IsOutOfAmmo;

        public Action<ShotFiredEvent> OnShotFired { get; set; }

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

        protected virtual void PerformRaycast()
        {
            var startPoint = MuzzleTransform.position;
            var endPoint = startPoint + MuzzleTransform.forward * Config.damageSettings.range;

            if (Physics.Raycast(startPoint, MuzzleTransform.forward, out var hit, Config.damageSettings.range,
                    ~LayerMask.GetMask("Ignore Raycast")))
            {
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, hit.point, hit));


                ApplyDamage(hit);
            }
            else
            {
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, endPoint, new RaycastHit()));
            }
        }

        protected void ApplyDamage(RaycastHit hit)
        {
            if (!hit.collider.transform.root.TryGetComponent<IDamageable>(out var damageable)) return;

            if (hit.collider.TryGetComponent<BodyPart>(out var bodyPart))
            {
                damageable.TakeDamage(Config.damageSettings.damage * bodyPart.damageMultiplier,
                    MuzzleTransform.forward);
                EventBus<GunFiredEvent>.Raise(new GunFiredEvent(Transform.position,
                    Config.damageSettings.damage));
            }
            else
            {
                damageable.TakeDamage(Config.damageSettings.damage, MuzzleTransform.forward);
            }
        }

        private void ProcessHits(int hitCount, Vector3 startPoint)
        {
            for (var i = 0; i < 1; i++)
            {
                var hit = HitResults[i];

                if (hit.collider == null) continue;
                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, hit.point, hit));


                if (!hit.collider.transform.root.TryGetComponent<IDamageable>(out var damageable) &&
                    !hit.collider.transform.root.TryGetComponent(out damageable)) continue;
                if (hit.collider.TryGetComponent<BodyPart>(out var bodyPart))
                {
                    damageable.TakeDamage(Config.damageSettings.damage * bodyPart.damageMultiplier,
                        MuzzleTransform.forward);
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