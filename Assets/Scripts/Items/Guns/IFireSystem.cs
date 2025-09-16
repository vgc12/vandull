using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items.Guns
{
    public interface IFireSystem
    {
        event Action<Vector3, float> OnFired;
        event Action OnFireModeChanged;
    
        bool CanFire { get; }
        void Fire(InputAction.CallbackContext context);
        void StopFire();
        void SetFireMode(FireType fireType);
        FireType CurrentFireType { get; }
        
        void Update();
        void CycleFireMode();
    }
}