using UnityEngine;

namespace Items.Guns
{
    [RequireComponent(typeof(Collider))]
    public sealed class BodyPart : MonoBehaviour
    {
        public float damageMultiplier = 1f;
    }
}