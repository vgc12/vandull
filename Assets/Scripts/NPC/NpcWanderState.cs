namespace NPC
{
    public abstract class NpcWanderState : NpcState
    {
        
        protected NpcWanderState(Npc enemy) : base(enemy)
        {
            
        }
        
        
        public override void Enter()
        {
            float range = 10f;
            int attempts = 30;
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