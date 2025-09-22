using General;
using UnityEngine;

namespace NPC.Strategies
{
    public class ConeDetectionStrategy : IDetectionStrategy
    {
        private readonly float _detectionAngle;
        private readonly float _detectionRadius;
        private readonly float _innerDetectionRadius;

        public ConeDetectionStrategy(float detectionAngle, float detectionRadius, float innerDetectionRadius)
        {
            _detectionAngle = detectionAngle;
            _detectionRadius = detectionRadius;
            _innerDetectionRadius = innerDetectionRadius;
        }

        public bool Execute(Transform player, Transform detector, CountdownTimer timer)
        {
            if (timer.IsRunning) return false;
            var directionToPlayer = (player.position - detector.position).normalized;
            var angleToPlayer = Vector3.Angle(detector.forward, directionToPlayer);
            
            // If the player is outside the detection angle or outside the detection radius, return false
            if(!(angleToPlayer < _detectionAngle / 2f) || !(directionToPlayer.magnitude < _detectionRadius)
               && !(directionToPlayer.magnitude < _innerDetectionRadius))
                return false;
            
            timer.Start();
            return true;
        }
    }
}