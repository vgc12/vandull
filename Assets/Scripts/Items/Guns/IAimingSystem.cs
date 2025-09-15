using System;
using UnityEngine;

namespace Items.Guns
{
    public interface IAimingSystem
    {
        event Action OnAimStarted;
        event Action OnAimStopped;
        
        bool IsAiming { get; }
        
        void StartAiming();
        
        void StopAiming();
        
        void Update();
        
    }
}