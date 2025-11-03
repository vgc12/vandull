using System;

namespace Items.Guns.Firing
{
    public enum FireCommand
    {
        StartAutomaticFire,
        StopAutomaticFire,
        SingleShot
    }

    public interface IFireSystem : IItemSystem
    {
        Action<ShotFiredEvent> OnShotFired { get; set; }
        void StopFire();
        void Fire();
    }
}