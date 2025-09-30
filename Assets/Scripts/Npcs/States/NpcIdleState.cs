using Npcs.Shared;

namespace Npcs.States
{
    public class NpcIdleState : NpcState
    {
        public NpcIdleState(Npc enemy) : base(enemy)
        {
        }

        public override void Enter()
        {
            Enemy.StopMoving();
            Enemy.IdleWaitBeforeMoving();
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