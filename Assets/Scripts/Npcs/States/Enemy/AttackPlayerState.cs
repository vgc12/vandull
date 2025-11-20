using Items.Guns;
using Items.Guns.Firing;

namespace Npcs.States.Enemy
{
    public class AttackPlayerState : NpcState
    {
        private readonly Gun _gun;
        private readonly Npcs.Enemy _npc;

        public AttackPlayerState(Npcs.Enemy npc) : base(npc)
        {
            _npc = npc;
            _gun = npc.Gun;
        }

        public override void Enter()
        {
            base.Enter();
            _npc.Gun.FireModeSystem.SetCurrentFireMode(FireType.Automatic);
        }

        public override void Update()
        {
            _npc.HandleMovementBlendTree();


            var sensor = _npc.PlayerSensor;

            _npc.Gun.StartAiming();
            _npc.LookAtTarget(sensor.Target.transform.position, _npc.LookAtSpeed);

            _npc.HandleTacticalMovement();

            if (_gun.AmmoSystem.OutOfAmmo)
            {
                _gun.AmmoSystem.StartReload();
                _gun.StopUse();
            }
            else
            {
                _gun.Use();
            }
        }

        public override void Exit()
        {
            _gun.StopAiming();
            _gun.StopUse();
        }
    }
}