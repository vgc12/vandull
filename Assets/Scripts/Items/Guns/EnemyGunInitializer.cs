using Items.Guns.Aiming;
using Items.Guns.Recoil;
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
            return builder.WithAimingSystem(() => new EnemyAimingSystem())
                .WithRecoilSystem(() => new EnemyRecoilSystem()).Build();
        }
    }
}