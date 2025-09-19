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
        [Required] public Transform gunHoldPoint;

        protected override void Awake()
        {
            base.Awake();
            gun.Equip();
        }

        protected override void InitializeStateMachine()
        {
          
            var walkState = new EnemyWalkState(this);
            var idleState = new EnemyIdleState(this);
            StateMachine.AddTransition(walkState, idleState,() => NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance && !NavMeshAgent.pathPending);
            StateMachine.AddTransition(idleState, walkState, () => CanWalk);
            StateMachine.SetState(idleState);
            
            //gun.hipFireTransform = gunHoldPoint;
            //gun.adsTransform = gunHoldPoint;
            
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