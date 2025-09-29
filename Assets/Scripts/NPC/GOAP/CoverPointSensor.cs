using UnityEngine;

namespace NPC.GOAP
{
    public class CoverPointSensor : MultiTargetTypeSensor<CoverPoint>
    {
        public CoverPoint GetClosestPointHiddenFrom(Vector3 point)
        {
            if (VisibleTargets.Count == 0)
                return null;

            CoverPoint bestCoverPoint = null;
            var closestDistance = float.MaxValue;

   
            var playerLayerMask = LayerMask.GetMask("Player");
            var rayOriginOffset = Vector3.up;

            foreach (var possiblePoint in VisibleTargets)
            {
                var coverPosition = possiblePoint.transform.position;
                var rayOrigin = coverPosition + rayOriginOffset;

              
                var direction = point - rayOrigin;
                var distanceSqr = direction.sqrMagnitude; // Use squared distance to avoid sqrt

           
#if UNITY_EDITOR
                Debug.DrawLine(rayOrigin, point, Color.red, 50f);
#endif

                // Check if point is hidden from threat
                if (!Physics.Raycast(rayOrigin, direction.normalized, direction.magnitude, playerLayerMask))
                    // This point is hidden AND closer than previous best
                    if (distanceSqr < closestDistance)
                    {
                        closestDistance = distanceSqr;
                        bestCoverPoint = possiblePoint;
                    }
            }

            return bestCoverPoint;
        }
    }
}