using System;
using System.Collections.Generic;
using Audio;
using DependencyInjection;
using EventBus;
using General;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace Items.Guns.Firing
{
    public abstract class BaseFireMode : IFireSystem
    {
        protected readonly Gun _gun;
        private readonly ILogger _logger;
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
            _logger = RuntimeResolver.Instance.Resolve<ILogger>();
            if (onShotFiredSubscribers == null)
            {
                return;
            }

            foreach (var subscriber in onShotFiredSubscribers)
            {
                OnShotFired += subscriber;
            }
        }

        public Transform MuzzleTransform { get; }

        public virtual bool FireRateTimeElapsed =>
            Time.time > LastFireTime + _gun.firingSettings.fireRate;


        public virtual bool OutOfAmmo => _gun.AmmoSystem.OutOfAmmo;

        public Action<ShotFiredEvent> OnShotFired { get; set; }


        public abstract void StopFire();

        public virtual void Update() { }

        public abstract void Fire();


        protected void PerformShot()
        {
            var sound = _gun.audioSettings.fire;
            if (!FireRateTimeElapsed || _gun.AmmoSystem.IsReloading || _gun.AmmoSystem.IsCheckingAmmo)
            {
                return;
            }

            if (OutOfAmmo)
            {
                LastFireTime = Time.time;
                sound = _gun.audioSettings.dryFire;
                AudioManager.Instance.PlaySfx(sound.clip, MuzzleTransform.position, pitch: sound.RandomPitch);
                return;
            }

            AudioManager.Instance.PlaySfx(sound.clip, MuzzleTransform.position, pitch: sound.RandomPitch);

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
            if (!hit.collider.transform.root.TryGetComponent<IDamageable>(out var damageable))
            {
                return;
            }

            if (hit.collider.TryGetComponent<BodyPart>(out var bodyPart))
            {
                damageable.TakeDamage(_gun.damageSettings.damage * bodyPart.damageMultiplier,
                    MuzzleTransform.forward, MuzzleTransform.position);
                EventBus<GunFiredEvent>.Raise(new GunFiredEvent(Transform.position,
                    _gun.damageSettings.damage));
            }
            else
            {
                damageable.TakeDamage(_gun.damageSettings.damage, MuzzleTransform.forward, MuzzleTransform.position);
            }
        }

        private void ProcessHits(int hitCount, Vector3 startPoint)
        {
            for (var i = 0; i < 1; i++)
            {
                var hit = HitResults[i];

                if (hit.collider == null)
                {
                    continue;
                }

                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, hit.point, hit));


                if (!hit.collider.transform.root.TryGetComponent<IDamageable>(out var damageable) &&
                    !hit.collider.transform.root.TryGetComponent(out damageable))
                {
                    continue;
                }

                if (hit.collider.TryGetComponent<BodyPart>(out var bodyPart))
                {
                    damageable.TakeDamage(_gun.damageSettings.damage * bodyPart.damageMultiplier,
                        MuzzleTransform.forward, MuzzleTransform.position);
                    EventBus<GunFiredEvent>.Raise(new GunFiredEvent(Transform.position,
                        _gun.damageSettings.damage));
                }
                else
                {
                    damageable.TakeDamage(_gun.damageSettings.damage, MuzzleTransform.forward,
                        MuzzleTransform.position);
                }
            }
        }
    }
}