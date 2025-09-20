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
        
        [SerializeField] private GameObject gunPrefab;
        private Gun gun;
        [Required] public Transform gunHoldPoint;
        public Vector3 gunRotationOffset;



        protected override void InitializeStateMachine()
        {
          
            var walkState = new EnemyWalkState(this);
            var idleState = new EnemyIdleState(this);
            StateMachine.AddTransition(walkState, idleState,() => !NavMeshAgent.isActiveAndEnabled ||( NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance && !NavMeshAgent.pathPending));
            StateMachine.AddTransition(idleState, walkState, () => CanWalk);
            StateMachine.SetState(idleState);

        
        }

        protected override void Awake()
        {
            base.Awake();
            var gunObject = Instantiate(gunPrefab, gunHoldPoint, false);
            gun = gunObject.GetComponent<Gun>();
            gun.Initialize(new GunInitializationData(gunHoldPoint, gunHoldPoint, gunHoldPoint, false));
            gun.Equip();
        }

        protected override void Update()
        {
            base.Update();
            gun.transform.rotation = gunHoldPoint.rotation;
        }
    }
}