using System;
using System.Collections.Generic;
using Audio;
using EventBus;
using General;
using UnityEngine;

namespace Items.Guns.Firing
{
    public abstract class BaseFireMode : IFireSystem
    {
        protected readonly Gun _gun;
        protected readonly MonoBehaviour Behaviour;
        protected readonly RaycastHit[] HitResults = new RaycastHit[10];
        protected readonly Transform Transform;


        protected float LastFireTime;

        protected BaseFireMode(Gun gun,
            List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
        {
            _gun = gun ?? throw new ArgumentNullException(nameof(gun));
            Transform = gun.transform;
            Behaviour = gun;
            MuzzleTransform = gun.muzzleTransform;

            if (onShotFiredSubscribers == null) return;
            foreach (var subscriber in onShotFiredSubscribers) OnShotFired += subscriber;
        }

        public Transform MuzzleTransform { get; }


        public abstract void ExecuteFireCommand(FireCommand command);

        public virtual bool CanFire =>
            Time.time > LastFireTime + _gun.firingSettings.fireRate && !_gun.AmmoSystem.OutOfAmmo;

        public Action<ShotFiredEvent> OnShotFired { get; set; }


        public abstract void StopFire();

        public virtual void Update()
        {
        }

        public abstract void Fire();


        protected void PerformShot()
        {
            if (!CanFire)
            {
                AudioManager.Instance.PlaySfx(_gun.audioSettings.outOfAmmoClick, MuzzleTransform.position);
                return;
            }

            AudioManager.Instance.PlaySfx(_gun.audioSettings.shoot, MuzzleTransform.position);

            LastFireTime = Time.time;
            PerformRaycast();
        }

        protected virtual void PerformRaycast()
        {
            var startPoint = MuzzleTransform.position;
            var endPoint = startPoint + MuzzleTransform.forward * _gun.damageSettings.range;

            if (Physics.Raycast(startPoint, MuzzleTransform.forward, out var hit, _gun.damageSettings.range,
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
                damageable.TakeDamage(_gun.damageSettings.damage * bodyPart.damageMultiplier,
                    MuzzleTransform.forward);
                EventBus<GunFiredEvent>.Raise(new GunFiredEvent(Transform.position,
                    _gun.damageSettings.damage));
            }
            else
            {
                damageable.TakeDamage(_gun.damageSettings.damage, MuzzleTransform.forward);
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
                    damageable.TakeDamage(_gun.damageSettings.damage * bodyPart.damageMultiplier,
                        MuzzleTransform.forward);
                    EventBus<GunFiredEvent>.Raise(new GunFiredEvent(Transform.position,
                        _gun.damageSettings.damage));
                }
                else
                {
                    damageable.TakeDamage(_gun.damageSettings.damage, MuzzleTransform.forward);
                }
            }
        }
    }
}