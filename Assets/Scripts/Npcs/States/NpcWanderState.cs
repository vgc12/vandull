using Npcs.Shared;

namespace Npcs.States
{
    public abstract class NpcWanderState : NpcState
    {
        protected NpcWanderState(Npc npc) : base(npc)
        {
        }


        public override void Enter()
        {
            if (Npc.NavMeshAgent.pathPending) return;

            const float range = 10f;
            const int attempts = 30;

            Npc.MoveToRandomPositionAtDistance(range, attempts);
        }


        public override void Update() => Npc.HandleMovementBlendTree();

        public override void FixedUpdate()
        {
        }

        public override void Exit() => Npc.StopMoving();
    }
}