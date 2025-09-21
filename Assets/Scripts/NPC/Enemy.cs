using System;
using Attributes;
using General;
using Items;
using Items.Guns;
using Items.Guns.Items.Guns;
using Items.Guns.Items.Guns.Builder;
using Items.Guns.Items.Guns.Dependencies;
using StateMachine;
using UnityEngine;

namespace NPC
{
    
    public class Enemy : Npc
    {
        
        [SerializeField] private GameObject gunPrefab;
        private Gun _gun;
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
             _gun = gunObject.GetComponent<Gun>();
            var dependencyContainer = new GunDependencyContainer(
                _gun.transform,
                _gun.gunConfig,
                this,
                gunHoldPoint,
                null,
                null);
            var gunBuilder = new GunSystemsBuilder(dependencyContainer);
            gunBuilder.WithAimingSystem(() => new EnemyAimingSystem())
                .WithRecoilSystem(() => new EnemyRecoilSystem());
            var gunSystems = gunBuilder.Build();
            _gun.Initialize(gunSystems);
            _gun.Equip();
        }

        protected override void Update()
        {
            base.Update();
            _gun.transform.rotation = gunHoldPoint.rotation;
        }
    }
}