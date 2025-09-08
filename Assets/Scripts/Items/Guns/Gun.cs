using System;
using System.Collections.Generic;
using Attributes;
using Player.Looking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items.Guns
{
    [RequireComponent(typeof(ObjectSwayer))]
    public class Gun : Item<GunConfig>
    {

        [SerializeField, Required] private Transform firePoint;
        [SerializeField, Required] private SwayConfig defaultConfig;
        [SerializeField, Required] private SwayConfig aimingConfig;
        
        private readonly RaycastHit[] _hitColliders = new RaycastHit[10];
        public ObjectSwayer objectSwayer;
        private StateMachine.StateMachine _stateMachine;
        private bool _firePressed;
        private bool _reloadPressed;
        public List<Magazine> Magazines { get; private set; } = new List<Magazine>();
        public Magazine CurrentMagazine { get; private set; }

        public int magazineIndex;
        private class GunStates
        {
            public GunIdleState IdleState { get; private init; }
            public GunAimingState AimingState { get; private init; }
            public GunFiringState FiringState { get; private init; }
            
            public GunStates(Gun gun)
            {
                IdleState = new GunIdleState(gun);
                AimingState = new GunAimingState(gun);
                FiringState = new GunFiringState(gun);
            }
        }
        protected override void Initialize()
        {
            base.Initialize();
      
            for(var i =0 ; i < itemConfig.magazineCount; i++)
            {
                Magazines.Add(new Magazine(itemConfig.magazineSize));
            }
            CurrentMagazine = Magazines[0];
            objectSwayer = GetComponent<ObjectSwayer>();
            var gunStates = new GunStates(this);
            _stateMachine = new StateMachine.StateMachine();
            
        }


        protected override void Use(InputAction.CallbackContext context)
        {
            _firePressed = context.performed;
        }


        private void Reload()
        {
            magazineIndex = (magazineIndex + 1) % Magazines.Count;
            CurrentMagazine = Magazines[magazineIndex];
        }

        private void Fire()
        {
            CurrentMagazine.SubtractOne();
            if (Physics.RaycastNonAlloc(firePoint.position, firePoint.forward, _hitColliders, itemConfig.range) <=
                0) return;
            foreach (var hitCollider in _hitColliders)
            {
                if (hitCollider.collider == null) continue;
                General.VandullLogger.Log($"Hit {hitCollider.collider.name} at distance {hitCollider.distance}");
                /*
                if (hitCollider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(itemConfig.damage);
                    Debug.Log($"Hit {hitCollider.name} for {itemConfig.damage} damage.");
                }
                */
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