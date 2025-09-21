using System;
using UnityEngine.InputSystem;

namespace Items.Guns
{
    public interface IFireSystem : IGunSystem
    {
    
        bool CanFire { get; }
        void Fire(InputAction.CallbackContext context);
        void StopFire();
        void SetFireMode(FireType fireType);
        FireType CurrentFireType { get; }
        
        void CycleFireMode();
        void OnOutOfAmmo();
    }
}