namespace Npcs.States.Enemy
{
    public sealed class EnemyDamagedState : NpcState
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