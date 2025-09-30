using Npcs.Shared;
using StateMachine;

namespace Npcs.States
{
    public class FindCoverState : NpcState
    {
        private readonly Npcs.Enemy _enemy;

        protected FindCoverState(Npcs.Enemy enemy) : base(enemy)
        {
            _enemy = enemy;
        }

        public override void Enter()
        {
            var point = _enemy.CoverPointSensor.GetClosestPointHiddenFrom(_enemy.PlayerSensor.Target.transform
                .position);
            if (point) _enemy.WalkToPoint(point.transform.position);
        }

        public override void Update()
        {
            if (_enemy.PlayerSensor.canSeeTarget)
                _enemy.LookAtTarget(_enemy.PlayerSensor.Target.transform.position, 15f);
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