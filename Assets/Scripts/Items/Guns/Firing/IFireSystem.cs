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
        bool CanFire { get; }

        Action<ShotFiredEvent> OnShotFired { get; set; }
        void ExecuteFireCommand(FireCommand command);
        void StopFire();
    }
}