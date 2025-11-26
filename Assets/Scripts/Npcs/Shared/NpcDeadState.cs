using Npcs.States;

namespace Npcs.Shared
{
    public sealed class EnemyDeadState : NpcState
    {
        private readonly Enemy _npc;

        public EnemyDeadState(Enemy npc) : base(npc)
        {
            _npc = npc;
        }

        public override void Enter()
        {
            _npc.Gun.StopUse();
            _npc.Gun.Drop();
            _npc.StopSensors();
        }
    }
}