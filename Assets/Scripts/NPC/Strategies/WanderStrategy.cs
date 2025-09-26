using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPC.GOAP
{
    public class WanderStrategy : IActionStrategy
    {
    
        private readonly NavMeshAgent _navMeshAgent;
        private readonly float _wanderRadius;
        
        
        public bool CanPerform => !Complete;
        public bool Complete => _navMeshAgent.remainingDistance <= 2f && !_navMeshAgent.pathPending;
        
     
    
        public WanderStrategy(NavMeshAgent navMeshAgent,  float wanderRadius)
        {
            _navMeshAgent = navMeshAgent;
            _wanderRadius = wanderRadius;
           
        }

        public void Start()
        { 
            MoveToRandomPositionAtDistance(_wanderRadius, 20);
            _navMeshAgent.speed = 2.0f;
            //WalkToRandomPoint(_wanderRadius);
        }
        
        public void WalkToRandomPoint(float range)
        {
            if (!_navMeshAgent.isActiveAndEnabled) return;
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * range;
            randomDirection += _navMeshAgent.transform.position;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, range, -1);
           _navMeshAgent.SetDestination(navHit.position);
        }
        
        public bool MoveToRandomPositionAtDistance(float targetDistance, int maxAttempts)
        {
            if (!_navMeshAgent.isActiveAndEnabled) return false;
            Vector3 startPosition = _navMeshAgent.transform.position;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Generate random direction
                Vector3 randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0; // Keep on same Y level


                // Calculate target position
                Vector3 targetPosition = startPosition + randomDirection * targetDistance;

                // Check if position is on NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
                {
                    // Verify the actual distance is close to desired
                    float actualDistance = Vector3.Distance(startPosition, hit.position);

                    if (Mathf.Abs(actualDistance - targetDistance) < 0.5f)
                    {
                        Debug.DrawLine(startPosition, hit.position, Color.red, 50);
                        _navMeshAgent.SetDestination(hit.position);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}