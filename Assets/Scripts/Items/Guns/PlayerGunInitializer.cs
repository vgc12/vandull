using UnityEngine;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "Player Gun Initializer", menuName = "Guns/Initialization/Player Gun Initializer",
        order = 1)]
    public sealed class PlayerGunInitializer : GunInitializer
    {
        public override GunSystems CreateGunSystems(Gun gun)
        {
            var builder = new Builder(gun);
            return builder.ForPlayer().Build();
        }
    }
}