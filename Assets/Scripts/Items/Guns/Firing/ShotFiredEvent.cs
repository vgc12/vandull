using EventBus;
using UnityEngine;

namespace Items.Guns
{
    public class ShotFiredEvent : IEvent
    {
        public Vector3 ShootPoint;
        public Vector3 EndPoint;
        public Vector3 Direction => (EndPoint - ShootPoint).normalized;
        public RaycastHit Hit;
        
        public ShotFiredEvent(Vector3 shootPoint, Vector3 endPoint, RaycastHit hit)
        {
            this.ShootPoint = shootPoint;
            this.EndPoint = endPoint;
            
            this.Hit = hit;
        }
        
    }
}