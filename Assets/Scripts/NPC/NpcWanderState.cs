using General;
using StateMachine;

namespace NPC
{
    public class NpcState : BaseState
    {
        protected readonly Npc Npc;

        protected NpcState(Npc npc)
        {
            Npc = npc;
        }
    }
    
    public abstract class NpcWanderState : NpcState
    {
        
        protected NpcWanderState(Npc npc) : base(npc)
        {
            
        }
        
        
        public override void Enter()
        {
            float range = 10f;
            int attempts = 30;
            Npc.MoveToRandomPositionAtDistance(range, attempts);
           
        }
        

        public override void Update()
        {
        
            
          // Npc.HandleAnimation();
        }

        public override void FixedUpdate()
        {
          
        }

        public override void Exit()
        {
           
        }
    }
}