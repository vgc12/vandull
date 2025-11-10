using Npcs.States;

namespace Npcs.Shared
{
    public class EnemyDeadState : NpcState
    {
        private readonly Enemy _enemy;

        public EnemyDeadState(Enemy enemy) : base(enemy)
        {
            _enemy = enemy;
        }

        public override void Enter()
        {
            _enemy.Gun.StopUse();
            _enemy.Gun.Drop();
        }
    }
}