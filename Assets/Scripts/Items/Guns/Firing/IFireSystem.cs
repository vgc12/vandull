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

    public interface IFireSystem : IItemSystem
    {
        public Transform MuzzleTransform { get; }
        bool CanFire { get; }

        Action<ShotFiredEvent> OnShotFired { get; set; }
        void ExecuteFireCommand(FireCommand command);
        void Fire();
        void StopFire();

        void OnOutOfAmmo();
        void OnReloadEnded();
    }
}