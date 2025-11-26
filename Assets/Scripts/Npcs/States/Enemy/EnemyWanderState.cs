using Npcs.Shared;

namespace Npcs.States.Enemy
{
    public sealed class EnemyWanderState : NpcWanderState
    {
        public EnemyWanderState(Npc npc) : base(npc)
        {
        }

        public override void Enter()
        {
            var walkPoint = ((Npcs.Enemy)Npc).WalkPatrolPointSensor.GetNextPatrolPoint();
            if (walkPoint) Npc.WalkToPoint(walkPoint.transform.position);
        }

        public override void Update()
        {
            base.Update();
            if (Npc.NavMeshAgent.remainingDistance <= Npc.NavMeshAgent.stoppingDistance &&
                !Npc.NavMeshAgent.pathPending) Npc.StopMoving();
        }
    }
}