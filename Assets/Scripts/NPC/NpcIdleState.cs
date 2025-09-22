using General;
using StateMachine;

namespace NPC
{
    public abstract class NpcIdleState : NpcState
    {
        protected readonly Npc Npc;

        protected NpcIdleState(Npc npc) : base(npc)
        {
            Npc = npc;
        }
        
        public override void Enter()
        {
            Npc.StopMoving();
            Npc.IdleWaitBeforeMoving();
      
        }

        public override void Update()
        {
           Npc.HandleAnimation();
        }

        public override void FixedUpdate()
        {
          
        }

        public override void Exit()
        {
           
        }
    }
}