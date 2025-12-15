using System;
using System.Collections.Generic;
using Audio;
using EventBus;
using General;
using Npcs;
using UnityEngine;

namespace Items.Guns.Firing
{
    public abstract class BaseFireMode : IFireSystem
    {
        protected readonly MonoBehaviour Behaviour;
        protected readonly Gun Gun;
        protected readonly Transform Transform;


        protected float LastFireTime;

        protected BaseFireMode(Gun gun,
            List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
        {
            Gun = gun ?? throw new ArgumentNullException(nameof(gun));
            Transform = gun.transform;
            Behaviour = gun;
            MuzzleTransform = gun.muzzleTransform;
            if (onShotFiredSubscribers == null) return;

            foreach (var subscriber in onShotFiredSubscribers) OnShotFired += subscriber;
        }

        public Transform MuzzleTransform { get; }

        public virtual bool FireRateTimeElapsed =>
            Time.time > LastFireTime + Gun.firingSettings.fireRate;


        public bool OutOfAmmo => Gun.AmmoSystem.OutOfAmmo;

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
            PerformRaycasts();
        }

        protected virtual void PerformRaycasts()
        {
            var startPoint = MuzzleTransform.position;
            var endPoint = startPoint + MuzzleTransform.forward * Gun.damageSettings.range;

            if (PerformEnemyHitRaycast(startPoint, endPoint))
            {
                return;
            }

            PerformSensorHitRaycast(startPoint);
        }

        private void PerformSensorHitRaycast(Vector3 startPoint)
        {
            //Raycast to see if this is in an enemies bullet sensor
            if (Physics.Raycast(startPoint, MuzzleTransform.forward, out var hit, Gun.damageSettings.range,
                    LayerMask.GetMask("ThreatZone")) &&
                hit.collider.transform.root.TryGetComponent<Enemy>(out var enemy))
            {
                EventBus<ThreatEvent>.Raise(new ThreatEvent(enemy, hit.point));
            }
        }

        //Returns a true if an enemy was hit else false
        private bool PerformEnemyHitRaycast(Vector3 startPoint, Vector3 endPoint)
        {
            if (Physics.Raycast(startPoint, MuzzleTransform.forward, out var hit, Gun.damageSettings.range,
                    ~LayerMask.GetMask("Ignore Raycast", "ThreatZone", "Gun",
                        Gun.Owner == OwnerStatus.Player ? "Player" : "Enemy")))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    Debug.LogWarning(
                        $"Gun {Gun.name} fired by {Gun.Owner} hit Player layer! This should be impossible.");
                }

                OnShotFired?.Invoke(new ShotFiredEvent(startPoint, Gun.trailStartPoint.position, hit.point, hit));

                ApplyDamage(hit);
                return true;
            }

            OnShotFired?.Invoke(
                new ShotFiredEvent(startPoint, Gun.trailStartPoint.position, endPoint, new RaycastHit()));
            return false;
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


                    AudioManager.Instance.PlaySfx(clip.clip, hit.point, spatialBlend: clip.spatialBlend,
                        pitch: randomRange);
                }

                damageable.TakeDamage(Gun.damageSettings.damage * bodyPart.damageMultiplier,
                    -MuzzleTransform.forward, MuzzleTransform);
                EventBus<GunFiredEvent>.Raise(new GunFiredEvent(Transform.position,
                    Gun.damageSettings.damage));
            }
            else
            {
                damageable.TakeDamage(Gun.damageSettings.damage, -MuzzleTransform.forward, MuzzleTransform);
            }
        }
    }
}