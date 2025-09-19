using System;
using Attributes;
using General;
using Items;
using Items.Guns;
using StateMachine;
using UnityEngine;

namespace NPC
{
    public class Enemy : Npc
    {
        
        [Required] public Gun gun;

        protected override void Awake()
        {
            base.Awake();
            gun.Spawn(this, true);
            gun.Equip();
        }

        protected override void InitializeStateMachine()
        {
          
            var walkState = new EnemyWalkState(this);
            var idleState = new EnemyIdleState(this);
            StateMachine.AddTransition(walkState, idleState,() => !CanWalk);
            StateMachine.AddTransition(idleState, walkState, () => CanWalk);
            StateMachine.SetState(idleState);
            
        }

        protected override void Update()
        {
            base.Update();
            gun.Update();
      
        }

        
        public override void Die()
        {
            
        }
    }
}