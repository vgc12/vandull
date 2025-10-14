using Npcs.Shared;

namespace Npcs.States
{
    public abstract class NpcWanderState : NpcState
    {
        protected NpcWanderState(Npc enemy) : base(enemy)
        {
        }


        public override void Enter()
        {
            if (Enemy.NavMeshAgent.pathPending) return;

            const float range = 10f;
            const int attempts = 30;

            Enemy.MoveToRandomPositionAtDistance(range, attempts);
        }


        public override void Update()
        {
            Enemy.HandleMovementBlendTree();
        }

        public override void FixedUpdate()
        {
        }

        public override void Exit()
        {
        }
    }
}