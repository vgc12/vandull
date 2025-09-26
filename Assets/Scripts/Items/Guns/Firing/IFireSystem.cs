using System;
using UnityEngine;

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

        public Transform MuzzleTransform { get; }
        event Action<ShotFiredEvent> OnShotFired;
        void ExecuteFireCommand(FireCommand command);
        bool CanFire { get; }
        void Fire();
        void StopFire();
        
        void OnOutOfAmmo();
        void OnReloadEnded();
    }
}