using UnityEngine;
using UnityEngine.AI;

namespace NPC
{
    public class EnemyWanderState : NpcWanderState
    {
        private readonly Enemy _npc;
        public EnemyWanderState(Enemy npc) : base(npc)
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
    
    
    public class EnemyChaseState : NpcState
    {
        private readonly Enemy _npc;
       
        private readonly NavMeshAgent _agent;
        private readonly Transform _player;
        
        public EnemyChaseState(Enemy npc,  NavMeshAgent agent, Transform player) : base(npc)
        {
            _npc = npc;
            _agent = agent;
            _player = player;
        }

        public override void Update()
        {
            base.Update();
            _agent.SetDestination(_player.position);
           // _npc.HandleAnimation();
        }
    }
}