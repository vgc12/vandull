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
            float range = 10f;
            int attempts = 30;
            Npc.MoveToRandomPositionAtDistance(range, attempts);
           
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