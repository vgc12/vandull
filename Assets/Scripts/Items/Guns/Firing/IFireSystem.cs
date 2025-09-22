using System;

namespace Items.Guns.Firing
{
    public enum FireCommand
    {
        StartAutomaticFire,
        StopAutomaticFire,
        SingleShot
    }
    
    public interface IFireSystem : IGunSystem
    {

        event Action<ShotFiredEvent> OnShotFired;
        void ExecuteFireCommand(FireCommand command);
        bool CanFire { get; }
        void Fire();
        void StopFire();
        
        void OnOutOfAmmo();
    }
}