using System;
using Items.Guns.Trail;
using UnityEngine;

namespace Items.Guns
{
    public interface IAimingSystem
    {
        public Transform HipFirePoint { get; }
        public Transform AimFirePoint { get; }
        
        event Action OnAimStarted;
        event Action OnAimStopped;
        
        bool IsAiming { get; }
        
        void StartAiming();
        
        void StopAiming();
        
        void Update();
        
    }
}