using Attributes;
using Items.Guns;
using NPC;
using NPC.GOAP;
using Player.States;
using UnityEngine;

namespace NPC
{
}

public class Enemy : Npc
    {
        [SerializeField,Required] public Gun gun;
        [SerializeField,Required] public Transform aimPoint;
        [SerializeField,Required] public EnemyObjectSensor playerSensor;
        [SerializeField, Required] public CoverPointSensor coverPointSensor;
    
        private float _engagementRange;

        
        protected override void InitializeStateMachine()
        {
            var idleState = new NpcIdleState(this);
            var wanderState = new EnemyWanderState(this);
            var attackState = new AttackPlayerState(this);
            StateMachine.AddAnyTransition(attackState, () => playerSensor.CanSeeTarget);
            StateMachine.AddTransition(attackState, idleState, () => !playerSensor.CanSeeTarget && !NavMeshAgent.pathPending);
            StateMachine.AddTransition(attackState, wanderState, () => !playerSensor.CanSeeTarget && NavMeshAgent.pathPending && NavMeshAgent.remainingDistance <= 1f);
           
        }
        
        
        public void HandleTacticalMovement()
        {
            var distanceToTarget = Vector3.Distance(NavMeshAgent.transform.position, playerSensor.Target.transform.position);


            if (NavMeshAgent.remainingDistance <= 2f || !NavMeshAgent.hasPath)
            {
                var strafePosition = GetStrafePosition(distanceToTarget);
                NavMeshAgent.SetDestination(strafePosition);
            }
        }

        private Vector3 GetStrafePosition(float currentDistance)
        {
            Vector3 targetPos = playerSensor.Target.transform.position;
            Vector3 currentPos = NavMeshAgent.transform.position;


            if (currentDistance < _engagementRange * 0.7f)
            {
                var awayDirection = (currentPos - targetPos).normalized;
                return targetPos + awayDirection * _engagementRange;
            }


            if (currentDistance > _engagementRange * 1.3f)
            {
                var towardDirection = (targetPos - currentPos).normalized;
                return currentPos + towardDirection * 5f;
            }
   

            var toTarget = (targetPos - currentPos).normalized;
            var rightDirection = Vector3.Cross(toTarget, Vector3.up).normalized;

   
            var strafeDirection = Random.value > 0.5f ? rightDirection : -rightDirection;
            return currentPos + strafeDirection * 8f;
        }

        public void LookAtTarget(Vector3 target, float turnSpeed)
        {
            var direction = target - gun.FireModeSystem.CurrentFireSystem.MuzzleTransform.position;
            NavMeshAgent.transform.rotation = Quaternion.Slerp(NavMeshAgent.transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * turnSpeed);
            NavMeshAgent.transform.rotation = Quaternion.Euler(0, NavMeshAgent.transform.rotation.eulerAngles.y, 0);
        }
    }

public class EnemyWanderState : NpcWanderState
{
    public EnemyWanderState(Npc enemy) : base(enemy)
    {
    }
}