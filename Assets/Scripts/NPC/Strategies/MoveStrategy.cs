using System;
using UnityEngine;
using UnityEngine.AI;

namespace NPC.GOAP
{
    public class MoveStrategy : IActionStrategy
    {
    
        private readonly NavMeshAgent _navMeshAgent;
        private readonly float _wanderRadius;
    
  
        public bool CanPerform => !Complete;
        public bool Complete => _navMeshAgent.remainingDistance <= 2f && !_navMeshAgent.pathPending;

        public readonly Func<Vector3> _destination;
    
        public MoveStrategy(NavMeshAgent navMeshAgent, Func<Vector3> destination)
        {
            _navMeshAgent = navMeshAgent;
            _destination = destination;
      
           
        }

        public void Start()
        { 
            MoveTo(_destination());
            // MoveToRandomPositionAtDistance(_wanderRadius, 20);
            //WalkToRandomPoint(_wanderRadius);
        }
        
        public void MoveTo(Vector3 position)
        {
            if (!_navMeshAgent.isActiveAndEnabled) return;
            _navMeshAgent.SetDestination(position);
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
        
        
    }
}