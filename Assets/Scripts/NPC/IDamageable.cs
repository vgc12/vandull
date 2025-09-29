using UnityEngine;

namespace NPC
{
    public interface IDamageable
    {
        
        void TakeDamage(float amount, Vector3 direction);
    }
}