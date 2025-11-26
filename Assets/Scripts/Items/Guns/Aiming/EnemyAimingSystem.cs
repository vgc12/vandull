using General;
using General.Extensions;
using Player.Movement;
using UnityEngine;

namespace Items.Guns.Aiming
{
    public sealed class EnemyAimingSystem : IAimingSystem
    {
        private readonly Transform _aimPoint;
        private Transform _target;


        public EnemyAimingSystem(Transform aimPoint, Transform target)
        {
            _aimPoint = aimPoint;
            _target = target;
        }

        public bool IsAiming { get; private set; }

        public void StartAiming()
        {
            IsAiming = true;
            if (!_target)
                _target = Object.FindFirstObjectByType<PlayerMovement>().GetComponentInChildren<Collider>().transform;
        }

        public void StopAiming()
        {
            IsAiming = false;
        }

        public void ResetPosition()
        {
        }


        public void Update()
        {
            if (IsAiming) _aimPoint.FollowPoint(_target.position, 20f);
        }
    }
}