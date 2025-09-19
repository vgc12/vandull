using General;
using StateMachine;

namespace NPC
{
    public abstract class NpcWalkState : BaseState
    {
        protected readonly Npc Npc;

        protected NpcWalkState(Npc npc)
        {
            Npc = npc;
        }
        
        public override void Enter()
        {
            Npc.WalkToRandomPoint(10f);
            VandullLogger.Log("Walk");
        }

        public override void Update()
        {
            Npc.CheckRemainingDistance();
            
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