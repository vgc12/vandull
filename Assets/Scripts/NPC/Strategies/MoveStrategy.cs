using System;
using General;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace NPC.GOAP
{
    public class MoveStrategy : IActionStrategy
    {
    
        private readonly NavMeshAgent _navMeshAgent;
        private readonly float _wanderRadius;
       
        
        public virtual bool CanPerform => !Complete;
        public virtual bool Complete
        {
            get => _navMeshAgent.remainingDistance <= 2f && !_navMeshAgent.pathPending;
            protected set => Complete = value;
        }


        public MoveStrategy(NavMeshAgent navMeshAgent, float wanderRadius = 10)
        {
            _navMeshAgent = navMeshAgent;
            _wanderRadius = wanderRadius;
         
        }

        public virtual void Update(float deltaTime){}
     
        public virtual void Start()
        { 
            VandullLogger.LogWarning("MoveStrategy Start");
            MoveToRandomPositionAtDistance(_wanderRadius, 20);
            _navMeshAgent.speed = 2.0f;
          
        }

        public void MoveTo(Vector3 targetPosition)
        {
            if(!_navMeshAgent.enabled) return;
            _navMeshAgent.SetDestination(targetPosition);
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

        public virtual void Stop(){}
        
        public void MoveToRandomPositionAtDistance(float targetDistance, int maxAttempts)
        {
            if (!_navMeshAgent.isActiveAndEnabled) return;
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
                        return;
                    }
                }
            }
        }
    }
}