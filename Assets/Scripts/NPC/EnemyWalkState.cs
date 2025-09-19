namespace NPC
{
    public class EnemyWalkState : NpcWalkState
    {
        private readonly Enemy _npc;
        public EnemyWalkState(Enemy npc) : base(npc)
        {
            _npc = npc;
        }

    }
    
    public class EnemyIdleState : NpcIdleState
    {
        private readonly Enemy _npc;
        public EnemyIdleState(Enemy npc) : base(npc)
        {
            _npc = npc;
        }
  
    }
}