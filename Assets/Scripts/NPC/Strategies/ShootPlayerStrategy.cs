using System;
using General;
using Items.Guns;
using NPC.GOAP;
using UnityEngine;
using UnityEngine.AI;

namespace NPC
{

    public class AttackPlayerState : NpcState
    {
        private readonly Enemy _enemy;
        private readonly Gun _gun;

        public AttackPlayerState(Enemy enemy) : base(enemy)
        {
            _enemy = enemy;
            _gun = enemy.gun;
        }

        public override void Enter()
        {
            base.Enter();
            _enemy.gun.FireModeSystem.SetCurrentFireMode(FireType.Automatic);
            _enemy.gun.StartAutomaticFire();

        }

        public override void Update()
        {
            _enemy.HandleMovementBlendTree();
            
            var aimPoint = _enemy.aimPoint;
            var sensor = _enemy.playerSensor;

            aimPoint.position = sensor.Target.transform.position;
            _enemy.LookAtTarget(sensor.Target.transform.position, 15f);
            
            _enemy.HandleTacticalMovement();
            
            if (_gun.AmmoSystem.CurrentMagazineEmpty)
            {
                _gun.StopAutomaticFire();
            }
            else
            {
                _gun.AmmoSystem.StartReload();
            }
        }
    }

}