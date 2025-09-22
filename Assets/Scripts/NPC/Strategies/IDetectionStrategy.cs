using General;
using UnityEngine;

namespace NPC
{
    public interface IDetectionStrategy
    {
        bool Execute(Transform player, Transform detector, CountdownTimer timer);
    }
}