using System;
using System.Collections.Generic;
using General;
using UnityEngine;
using UnityEngine.AI;

namespace NPC.GOAP
{
    public class FleeStrategy : IActionStrategy
    {
        private readonly NavMeshAgent _navMeshAgent;
        private readonly float _range;
        private readonly Func<List<Vector3>> _coverPoints;
        private readonly Func<bool> _isPlayerInSight;
        
        public bool CanPerform => !Complete;
        public bool Complete => _navMeshAgent.remainingDistance <= 2f && !_navMeshAgent.pathPending ;
        
        private Func<Vector3> _fleePoint;
        
        private List<Vector3> closestCoverPoints = new List<Vector3>();


        public FleeStrategy(NavMeshAgent navMeshAgent, Func<bool> isPlayerInSight, Func<List<Vector3>>coverPoints, Func<Vector3> fleePoint = null)
        {
            _navMeshAgent = navMeshAgent;
            
            _isPlayerInSight = isPlayerInSight;

            _coverPoints = coverPoints;
            
            _fleePoint = fleePoint ?? (() => Vector3.zero);
        }

       

        public void Start()
        {
            VandullLogger.LogWarning("FleeStrategy Start");
            _navMeshAgent.speed = 2.5f;

            GetClosestPointsInOrder();
            
            Vector3 point = Vector3.zero;
            for (var i = 0; i < closestCoverPoints.Count; i++)
            {
                var coverPoint = closestCoverPoints[i];
                var fleePoint = _fleePoint();
                VandullLogger.Log("fleePoint: " + fleePoint);
                Vector3 direction = coverPoint - fleePoint;
                float distance = direction.magnitude;
                Debug.DrawLine(coverPoint + Vector3.up, fleePoint, Color.red, 50f);
                if (Physics.Raycast(coverPoint + Vector3.up, direction.normalized, float.MaxValue,
                        LayerMask.GetMask("Player")))
                {
                    if(i == closestCoverPoints.Count - 1)
                        point = coverPoint;
                    
                    continue;
                }

                point = coverPoint;
            }

            _navMeshAgent.SetDestination(point);
        }
        
        
        public void GetClosestPointsInOrder()
        {
            closestCoverPoints.Clear();
            var coverPoints = _coverPoints();
            coverPoints.Sort((a, b) =>
            {
                float distA = Vector3.Distance(_navMeshAgent.transform.position, a);
                float distB = Vector3.Distance(_navMeshAgent.transform.position, b);
                return distA.CompareTo(distB);
            });
            closestCoverPoints.AddRange(coverPoints);
        }

       
    }
}