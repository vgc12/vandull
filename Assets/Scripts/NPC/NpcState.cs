using StateMachine;

namespace NPC
{
    public class FindCoverState : NpcState
    {
        private readonly Enemy _enemy;

        protected FindCoverState(Enemy enemy) : base(enemy)
        {
            _enemy = enemy;
        }

        public override void Enter()
        {
            var point = _enemy.coverPointSensor.GetClosestPointHiddenFrom(_enemy.playerSensor.Target.transform
                .position);
            if (point) _enemy.WalkToPoint(point.transform.position);
        }

        public override void Update()
        {
            if (_enemy.playerSensor.canSeeTarget)
                _enemy.LookAtTarget(_enemy.playerSensor.Target.transform.position, 15f);
        }
    }

    public class NpcState : BaseState
    {
        protected readonly Npc Enemy;

        protected NpcState(Npc enemy)
        {
            Enemy = enemy;
        }
    }
}