using Items.Guns;
using Items.Guns.Firing;

namespace Npcs.States.Enemy
{
    public class AttackPlayerState : NpcState
    {
        private readonly Npcs.Enemy _enemy;
        private readonly Gun _gun;

        public AttackPlayerState(Npcs.Enemy enemy) : base(enemy)
        {
            _enemy = enemy;
            _gun = enemy.Gun;
        }

        public override void Enter()
        {
            base.Enter();
            _enemy.Gun.FireModeSystem.SetCurrentFireMode(FireType.Automatic);
        }

        public override void Update()
        {
            _enemy.Gun.StartAutomaticFire();
            _enemy.HandleMovementBlendTree();

            var aimPoint = _enemy.AimPoint;
            var sensor = _enemy.PlayerSensor;

            _enemy.Gun.StartAiming();
            _enemy.LookAtTarget(sensor.Target.transform.position, _enemy.LookAtSpeed);

            _enemy.HandleTacticalMovement();

            if (_gun.AmmoSystem.CurrentMagazineEmpty) _gun.AmmoSystem.StartReload();
        }

        public override void Exit()
        {
            _gun.StopAiming();
            _gun.StopAutomaticFire();
        }
    }
}