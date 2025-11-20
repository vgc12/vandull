using Npcs.Shared;

namespace Npcs.States
{
    public class NpcIdleState : NpcState
    {
        public NpcIdleState(Npc npc) : base(npc)
        {
        }

        public override void Enter()
        {
            Npc.StopMoving();
            Npc.IdleWaitBeforeMoving();
        }

        public override void Update()
        {
            Npc.HandleMovementBlendTree();
        }

        public override void FixedUpdate()
        {
        }

        public override void Exit()
        {
        }
    }
}