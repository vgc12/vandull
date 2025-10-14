using EventBus;
using UnityEngine;

namespace Items.Guns.Firing
{
    public class ShotFiredEvent : IEvent
    {
        public Vector3 EndPoint;
        public RaycastHit Hit;
        public Vector3 ShootPoint;

        public ShotFiredEvent(Vector3 shootPoint, Vector3 endPoint, RaycastHit hit)
        {
            ShootPoint = shootPoint;
            EndPoint = endPoint;

            Hit = hit;
        }

        public Vector3 Direction => (EndPoint - ShootPoint).normalized;
    }
}