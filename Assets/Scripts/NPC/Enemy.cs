using System;
using Attributes;
using Items;
using Items.Guns;
using Items.Guns.Items.Guns;
using Items.Guns.Items.Guns.Builder;
using Items.Guns.Items.Guns.Dependencies;
using StateMachine;
using UnityEngine;
using UnityEngine.AI;

namespace NPC
{
    [RequireComponent(typeof(PlayerDetector))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : Npc
    {
        
        [SerializeField] private GameObject gunPrefab;
        [SerializeField,Required] private PlayerDetector playerDetector; 
        [Required] public Transform gunHoldPoint;
        private Gun _gun;
   
        public Vector3 gunRotationOffset;



        protected override void InitializeStateMachine()
        {
          
            var walkState = new EnemyWanderState(this);
            var idleState = new EnemyIdleState(this);
            var chaseState = new EnemyChaseState(this, NavMeshAgent, playerDetector.Player);
            StateMachine.AddTransition(walkState, idleState,() => !NavMeshAgent.isActiveAndEnabled ||( NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance && !NavMeshAgent.pathPending));
            StateMachine.AddTransition(idleState, walkState, () => CanWalk);
            StateMachine.AddAnyTransition(chaseState, playerDetector.CanDetectPlayer);
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
                gunHoldPoint);
            /*
            var gunBuilder = new GunSystemsBuilder(dependencyContainer);
            gunBuilder.WithAimingSystem(() => new EnemyAimingSystem())
                .WithRecoilSystem(() => new EnemyRecoilSystem());
            var gunSystems = gunBuilder.Build();
            */
            var initializer = new Gun.Initializer(_gun, dependencyContainer);
            initializer.WithAimingSystem(() => new EnemyAimingSystem())
                .WithRecoilSystem(() => new EnemyRecoilSystem())
                .Initialize();
            
            _gun.Equip();
        }

        protected override void Update()
        {
            base.Update();
            _gun.transform.rotation = gunHoldPoint.rotation;
        }
        
        public void Attack()
        {
            
        }
    }
}