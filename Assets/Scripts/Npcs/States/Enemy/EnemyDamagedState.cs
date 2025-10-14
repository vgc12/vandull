using General;

namespace Npcs.States.Enemy
{
    public class EnemyDamagedState : NpcState
    {
        private readonly Npcs.Enemy _enemy;

        public EnemyDamagedState(Npcs.Enemy enemy) : base(enemy)
        {
            _enemy = enemy;
        }

        public override void Update()
        {
            VandullLogger.Log("Entered damage state, turning towards player");
            _enemy.LookAtDamageDirection();
        }
    }
}