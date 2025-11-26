using EventBus;
using Levels.Strategies;
using Npcs;

namespace Levels
{
    public sealed class AssassinationTarget : Enemy
    {
        public override void Die()
        {
            EventBus<TargetEnemyKilledEvent>.Raise(new TargetEnemyKilledEvent(this, transform.position));
            base.Die();
        }
    }
}