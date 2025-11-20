namespace Npcs.States.Enemy
{
    public class EnemyDamagedState : NpcState
    {
        private readonly Npcs.Enemy _npc;

        public EnemyDamagedState(Npcs.Enemy npc) : base(npc)
        {
            _npc = npc;
        }

        public override void Update()
        {
            _npc.LookAtDamageDirection();
        }
    }
}