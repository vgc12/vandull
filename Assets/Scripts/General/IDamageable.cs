using UnityEngine;

namespace General
{
    public interface IDamageable
    {
        void TakeDamage(float amount, Vector3 direction);
    }
}