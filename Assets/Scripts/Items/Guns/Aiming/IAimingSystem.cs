using System;
using Items.Guns.Trail;
using UnityEngine;

namespace Items.Guns
{
    public interface IAimingSystem : IGunSystem
    {
        bool IsAiming { get; }
        
        void StartAiming();
        
        void StopAiming();
        
    }
}