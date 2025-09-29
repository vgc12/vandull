using System;
using System.Collections.Generic;
using General;
using UnityEngine;
using UnityEngine.AI;

namespace NPC.GOAP
{
    public class FleeStrategy : MoveStrategy
    {
        private readonly NavMeshAgent _navMeshAgent;
        private readonly float _range;
        private readonly CoverPointSensor _sensor;
        
        private readonly EnemyObjectSensor _enemyObjectSensor;

        private List<CoverPoint> _closestCoverPoints = new();


        public FleeStrategy(NavMeshAgent navMeshAgent, CoverPointSensor sensor,
            EnemyObjectSensor enemyObjectSensor) : base(navMeshAgent)
        {
            _navMeshAgent = navMeshAgent;


            _sensor = sensor;

            _enemyObjectSensor = enemyObjectSensor;
        }


        public override void Start()
        {
            VandullLogger.LogWarning("FleeStrategy Start");
            _navMeshAgent.speed = 2.5f;

           GetClosestPointsInOrder();

            Vector3 point = Vector3.zero;
            
            for (var i = 0; i < _closestCoverPoints.Count; i++)
            {
                var coverPoint = _closestCoverPoints[i].transform.position;
                var fleePoint = _enemyObjectSensor.Target.transform.position;
                VandullLogger.Log("fleePoint: " + fleePoint);
                Vector3 direction = coverPoint - fleePoint;
                float distance = direction.magnitude;
                Debug.DrawLine(coverPoint + Vector3.up, fleePoint, Color.red, 50f);
                if (Physics.Raycast(coverPoint + Vector3.up, direction.normalized, float.MaxValue,
                        LayerMask.GetMask("Player")))
                {
                    if (i == _closestCoverPoints.Count - 1)
                        point = coverPoint;

                    continue;
                }

                point = coverPoint;
            }

            if (_navMeshAgent.enabled)
            {
                _navMeshAgent.SetDestination(point);
            }
            
        }

        public void GetClosestPointsInOrder()
        {
            _closestCoverPoints.Clear();
            var coverPoints = _sensor.AllDetectedTargets;
            coverPoints.Sort((a, b) =>
            {
                float distA = Vector3.Distance(_navMeshAgent.transform.position, a.transform.position);
                float distB = Vector3.Distance(_navMeshAgent.transform.position, b.transform.position);
                return distA.CompareTo(distB);
            });
            _closestCoverPoints.AddRange(coverPoints);
        }
        
    }
}