using EventBus;
using UnityEngine;

namespace Items.Guns.Firing
{
    public sealed class ShotFiredEvent : IEvent
    {
        public Vector3 EndPoint;
        public RaycastHit Hit;
        public Vector3 MuzzlePoint;
        public Vector3 RaycastPoint;

        public ShotFiredEvent(Vector3 raycastPoint, Vector3 muzzlePoint, Vector3 endPoint, RaycastHit hit)
        {
            RaycastPoint = raycastPoint;
            EndPoint = endPoint;
            MuzzlePoint = muzzlePoint;
            Hit = hit;
        }

        public Vector3 Direction => (EndPoint - RaycastPoint).normalized;
    }
}