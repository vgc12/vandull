using System;
using System.Collections;
using System.Collections.Generic;
using Attributes;
using General;
using Player.Looking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items.Guns
{
    [RequireComponent(typeof(ObjectSwayer))]
    public class Gun : Item<GunConfig>
    {
        [SerializeField, Required] private Transform muzzlePoint;
        [SerializeField] private Vector3 adsPosition;
        [SerializeField] private Vector3 hipFirePoint;
        [SerializeField, Required] private SwayConfig defaultConfig;
        [SerializeField, Required] private SwayConfig aimingConfig;

        private readonly RaycastHit[] _hitColliders = new RaycastHit[10];
        public ObjectSwayer ObjectSwayer { get; private set; }
        private StateMachine.StateMachine _stateMachine;
        private bool _firePressed;

        private bool _reloadPressed;
        private bool _reloading;

        private Coroutine _aimingCoroutine;

        public List<Magazine> Magazines { get; private set; } = new List<Magazine>();
        public Magazine CurrentMagazine { get; private set; }

        public int magazineIndex;

        private class GunStates
        {
            public GunIdleState IdleState { get; private init; }

            public GunFiringState FiringState { get; private init; }
            public GunReloadingState ReloadingState { get; set; }

            public GunStates(Gun gun)
            {
                IdleState = new GunIdleState(gun);

                ReloadingState = new GunReloadingState(gun);
                FiringState = new GunFiringState(gun);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(muzzlePoint.position, muzzlePoint.forward * itemConfig.range);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(adsPosition + transform.position, 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(hipFirePoint + transform.position, 0.1f);
        }

        protected override void Initialize()
        {
            base.Initialize();

            for (var i = 0; i < itemConfig.magazineCount; i++)
            {
                Magazines.Add(new Magazine(itemConfig.magazineSize));
            }

            CurrentMagazine = Magazines[0];
            ObjectSwayer = GetComponent<ObjectSwayer>();
            var gunStates = new GunStates(this);
            _stateMachine = new StateMachine.StateMachine();
            _stateMachine.AddAnyTransition(gunStates.IdleState, () => !_firePressed && !_reloading);
            _stateMachine.AddAnyTransition(gunStates.FiringState,
                () => _firePressed && !_reloading && !CurrentMagazine.IsEmpty);
            _stateMachine.AddAnyTransition(gunStates.ReloadingState, () => !_reloading);
            _stateMachine.SetState(gunStates.IdleState);
        }


        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }
        
        protected override void Use(InputAction.CallbackContext context)
        {
            if (itemConfig.fireType == FireType.SemiAutomatic)
            {
                _firePressed = context.started;
                return;
            }

            _firePressed = context.performed;
        }


        public void Reload()
        {
            magazineIndex = (magazineIndex + 1) % Magazines.Count;
            CurrentMagazine = Magazines[magazineIndex];
        }


        public void UnAim()
        {
            if (_aimingCoroutine != null) StopCoroutine(_aimingCoroutine);
            _aimingCoroutine = StartCoroutine(LerpToPoint(hipFirePoint));
        }

        public void Aim()
        {
            if (_aimingCoroutine != null) StopCoroutine(_aimingCoroutine);
            _aimingCoroutine = StartCoroutine(LerpToPoint(adsPosition));
        }

        private IEnumerator LerpToPoint(Vector3 targetPosition)
        {
            var elapsed = 0f;
            var duration = itemConfig.adsTime;
            var initialPosition = transform.localPosition;
            while (elapsed < duration)
            {
                transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = adsPosition;
        }

        public void Fire()
        {
            CurrentMagazine.SubtractOne();
            if (Physics.RaycastNonAlloc(muzzlePoint.position, muzzlePoint.forward, _hitColliders, itemConfig.range) <=
                0) return;
            foreach (var hitCollider in _hitColliders)
            {
                if (hitCollider.collider == null) continue;
                VandullLogger.Log($"Hit {hitCollider.collider.name} at distance {hitCollider.distance}");
                /*
                if (hitCollider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(itemConfig.damage);
                    Debug.Log($"Hit {hitCollider.name} for {itemConfig.damage} damage.");
                }
                */
            }
        }


        private IEnumerator FireCooldown(float cooldown)
        {
            float timer = itemConfig.fireRate;
            while (timer > 0)
            {
                yield return new WaitForSeconds(cooldown);
            }
        }
    }

    public class Magazine
    {
        public int CurrentAmmo { get; private set; }

        private int Capacity { get; }

        public Magazine(int capacity)
        {
            Capacity = capacity;
            CurrentAmmo = capacity;
        }

        public bool IsEmpty => CurrentAmmo <= 0;
        public bool IsFull => CurrentAmmo >= Capacity;

        public void SubtractAmmo(int amount)
        {
            CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        }

        public void SubtractOne()
        {
            SubtractAmmo(1);
        }
    }
}