using UnityEngine;

namespace NPC.GOAP
{
    public class TargetChangedEventArgs
    {
        public TargetChangeReason Reason { get; }
        public GameObject PreviousTarget { get; }
        public GameObject CurrentTarget { get; }

        public TargetChangedEventArgs(TargetChangeReason reason, GameObject previous, GameObject current)
        {
            Reason = reason;
            PreviousTarget = previous;
            CurrentTarget = current;
        }
    }
}