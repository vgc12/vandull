using EventBus;
using UnityEngine;

namespace Items.Guns
{
    public class ShotHitEvent : IEvent
    {
        public RaycastHit Hit;

        public ShotHitEvent(RaycastHit hit)
        {
            Hit = hit;
        }
    }
}