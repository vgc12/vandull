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
        protected readonly Gun Gun;
        private readonly ILogger _logger;
        protected readonly MonoBehaviour Behaviour;
        protected readonly RaycastHit[] HitResults = new RaycastHit[10];
        protected readonly Transform Transform;


        protected float LastFireTime;

        protected BaseFireMode(Gun gun,
            List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
        {
            Gun = gun ?? throw new ArgumentNullException(nameof(gun));
            Transform = gun.transform;
            Behaviour = gun;
            MuzzleTransform = gun.muzzleTransform;
            _logger = RuntimeResolver.Instance.Resolve<ILogger>();
            if (onShotFiredSubscribers == null) return;

            foreach (var subscriber in onShotFiredSubscribers) OnShotFired += subscriber;
        }

        public Transform MuzzleTransform { get; }

        public virtual bool FireRateTimeElapsed =>
            Time.time > LastFireTime + Gun.firingSettings.fireRate;


        public virtual bool OutOfAmmo => Gun.AmmoSystem.OutOfAmmo;

        public Action<ShotFiredEvent> OnShotFired { get; set; }


        public abstract void StopFire();

        public virtual void Update()
        {
        }

        public abstract void Fire();


        protected void PerformShot()
        {
            var sound = Gun.audioSettings.fire;
            if (!FireRateTimeElapsed || Gun.AmmoSystem.IsReloading || Gun.AmmoSystem.IsCheckingAmmo) return;

            if (OutOfAmmo)
            {
                LastFireTime = Time.time;
                sound = Gun.audioSettings.dryFire;
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
            var endPoint = startPoint + MuzzleTransform.forward * Gun.damageSettings.range;

            if (Physics.Raycast(startPoint, MuzzleTransform.forward, out var hit, Gun.damageSettings.range,
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
                //This is probably the head of the enemy or at least a critical part either way it should make the crit sound

                var clip = Gun.audioSettings.bodyPartHit;
                if (clip != null)
                {
                    if (bodyPart.damageMultiplier > 2f) clip = Gun.audioSettings.headPartHit;

                    var randomRange = clip.RandomPitch;


                    AudioManager.Instance.PlaySfx(clip.clip, hit.point, spatialBlend: clip.spatialBlend, pitch: randomRange);
                }

                damageable.TakeDamage(Gun.damageSettings.damage * bodyPart.damageMultiplier,
                    MuzzleTransform.forward, MuzzleTransform);
                EventBus<GunFiredEvent>.Raise(new GunFiredEvent(Transform.position,
                    Gun.damageSettings.damage));
            }
            else
            {
                damageable.TakeDamage(Gun.damageSettings.damage, MuzzleTransform.forward, MuzzleTransform);
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
                    !hit.collider.transform.root.TryGetComponent(out damageable))
                    continue;

                if (hit.collider.TryGetComponent<BodyPart>(out var bodyPart))
                {
                    damageable.TakeDamage(Gun.damageSettings.damage * bodyPart.damageMultiplier,
                        MuzzleTransform.forward, MuzzleTransform);
                    EventBus<GunFiredEvent>.Raise(new GunFiredEvent(Transform.position,
                        Gun.damageSettings.damage));
                }
                else
                {
                    damageable.TakeDamage(Gun.damageSettings.damage, MuzzleTransform.forward,
                        MuzzleTransform);
                }
            }
        }
    }
}