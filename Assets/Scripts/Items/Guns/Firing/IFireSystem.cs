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
        bool CanFire { get; }
        event Action<ShotFiredEvent> OnShotFired;
        void ExecuteFireCommand(FireCommand command);
        void Fire();
        void StopFire();

        void OnOutOfAmmo();
        void OnReloadEnded();
    }
}