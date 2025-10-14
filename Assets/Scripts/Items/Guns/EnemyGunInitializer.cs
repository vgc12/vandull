using Items.Guns.Aiming;
using Items.Guns.Firing;
using Items.Guns.Recoil;
using Npcs.Sensors;
using Player.Movement;
using UnityEngine;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "Enemy Gun Initializer", menuName = "Guns/Initialization/Enemy Gun Initializer",
        order = 1)]
    public class EnemyGunInitializer : GunInitializer
    {
        public ISensor Sensor;

        public override GunSystems CreateGunSystems(Gun gun)
        {
            var builder = new Builder(gun);
            return builder.WithCustomFireMode(FireType.Automatic,
                    () => new EnemyAutomaticFireMode(gun.gunConfig, gun.transform, gun, gun.muzzleTransform))
                .WithRecoilSystem(() =>
                    new EnemyRecoilSystem(gun.gunConfig, gun.recoilTransform, gun))
                .WithAimingSystem(() => new EnemyAimingSystem(gun.aimTransform,
                    FindFirstObjectByType<PlayerMovement>().GetComponentInChildren<Collider>().transform)).Build();
        }
    }
}