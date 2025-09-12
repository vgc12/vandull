using UnityEngine;

namespace Items.Guns
{
    public interface IAimingSystem
    {
        bool IsAiming { get; }
        
        void StartAiming(Vector3 adsPosition, Vector3 hipPosition);
        
        void StopAiming();
        
        void Update();
        
    }
}