using Items.Guns.Aiming;
using Items.Guns.Firing;
using Items.Guns.Recoil;
using Player.Movement;
using UnityEngine;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "Enemy Gun Initializer", menuName = "Guns/Initialization/Enemy Gun Initializer",
        order = 1)]
    public class EnemyGunInitializer : GunInitializer
    {
        public override GunSystems CreateGunSystems(Gun gun)
        {
            var builder = new Builder(gun);
            return builder.WithCustomFireMode(FireType.Automatic,
                    () => new EnemyAutomaticFireMode(gun))
                .WithRecoilSystem(() =>
                    new NullRecoilSystem())
                .WithAimingSystem(() => new EnemyAimingSystem(gun.aimTransform,
                    FindFirstObjectByType<PlayerMovement>().GetComponentInChildren<Collider>().transform))
                .WithAnimationSystem(() => new EnemyGunAnimationSystem())
                .WithOwnerStatus(OwnerStatus.Enemy)
                .Build();
        }
    }
}