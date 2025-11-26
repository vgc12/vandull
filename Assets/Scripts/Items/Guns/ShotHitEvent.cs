using EventBus;
using UnityEngine;

namespace Items.Guns
{
    public sealed class ShotHitEvent : IEvent
    {
        public RaycastHit Hit;

        public ShotHitEvent(RaycastHit hit)
        {
            Hit = hit;
        }
    }
}