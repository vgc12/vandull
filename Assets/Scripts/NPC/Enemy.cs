using System;
using Attributes;
using General;
using Items.Guns;
using Items.Guns.Firing;
using Items.Guns.Items.Guns.Dependencies;
using NPC.GOAP;
using UnityEngine;
using UnityEngine.AI;

namespace NPC
{
    public class Enemy : GoapAgent
    {
        
        [SerializeField, Required] private Gun gun;
        [SerializeField, Required] private Transform aimPoint;

        protected override void Start()
        {
            
            Gun.Initializer initializer = new(gun);
            initializer.
                WithAimingSystem(() => new EnemyAimingSystem()).
                WithRecoilSystem(() => new EnemyRecoilSystem())
                .Initialize();
            base.Start();
          
        }


        protected override void SetupActions()
        {
            base.SetupActions();
            Actions.Add(new AgentAction.Builder("Flee")
                .AddPrecondition(Beliefs[BeliefType.IsNotSafe])
                .WithStrategy(new FleeStrategy(NavMeshAgent, () => playerRaycastSensor.IsTargetPresent,
                    () => coverPointSensor.TargetPositions, () => playerRaycastSensor.TargetPosition))
                .AddEffect(Beliefs[BeliefType.IsSafe])
                .Build());
            Actions.Add(new AgentAction.Builder("Shoot Player").AddPrecondition(Beliefs[BeliefType.HasAmmo])
                .AddPrecondition(Beliefs[BeliefType.PlayerAlive])
                .AddPrecondition(Beliefs[BeliefType.HasAmmo])
                .WithStrategy(new ShootPlayerStrategy(NavMeshAgent, AnimationController,gun, aimPoint,() => playerRaycastSensor.IsTargetPresent,
                    () => playerRaycastSensor.TargetPosition))
                .AddEffect(Beliefs[BeliefType.IsSafe])
                .AddEffect(Beliefs[BeliefType.PlayerDead])
                .Build());
        }

        protected override void SetupGoals()
        {
            base.SetupGoals();
            Goals.Add(new AgentGoal.Builder("Kill Player") 
                .WithDesiredEffect(Beliefs[BeliefType.PlayerDead])
                .WithPriority(2)
                .Build());
        }


        protected override void SetupBeliefs()
        {
            base.SetupBeliefs();
            Factory.AddBelief(BeliefType.HasAmmo, () => !gun.AmmoSystem.IsCurrentMagazineEmpty);
            Factory.AddBelief(BeliefType.PlayerDead, () => playerRaycastSensor.TargetComponent?.Health <= 0);
            Factory.AddBelief(BeliefType.PlayerAlive, () =>  playerRaycastSensor.TargetComponent?.Health > 0);
        }

        private void OnDrawGizmos()
        {
            if (Beliefs == null) return;
            if (Beliefs.TryGetValue(BeliefType.CoverInRange, out var belief))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(belief.Location, 0.3f);
            }
        }
    }

    public class ShootPlayerStrategy : IActionStrategy
    {
        private readonly Gun _gun;
        private readonly NavMeshAgent _navMesh;
        private readonly Func<bool> _canSeeTarget;
        private readonly Func<Vector3> _target;
        private readonly AnimationController _animationController;
        private readonly Transform _aimPoint;
        
        public ShootPlayerStrategy(NavMeshAgent navMesh,AnimationController animationController,  Gun gun, Transform aimPoint, Func<bool> canSeeTarget, Func<Vector3> target)
        {
            _navMesh = navMesh;
            _gun = gun;
            _canSeeTarget = canSeeTarget;
            _target = target;
            _animationController = animationController;
            _aimPoint = aimPoint;

        }

        public void Start()
        {
            VandullLogger.LogWarning("Starting to shoot at player");
            _gun.StartAutomaticFire();
     
            
        }


        public void Update(float deltaTime)
        {
            if (!_canSeeTarget())
            {
           
                return;
            }
            
            
            _navMesh.isStopped = true;
            var direction = _target() - _gun.FireModeSystem.CurrentFireSystem.MuzzleTransform.position;
            _navMesh.transform.rotation = Quaternion.Slerp(_navMesh.transform.rotation, Quaternion.LookRotation(direction), deltaTime * 10f);
            _navMesh.transform.rotation = Quaternion.Euler(0, _navMesh.transform.rotation.eulerAngles.y, 0);
       
            _aimPoint.position = _target();
      
            VandullLogger.LogWarning("Shooting at player");
      
        }
        public void Stop()
        {
            VandullLogger.LogWarning("Stopping shooting at player");
            _gun.StopAutomaticFire();
            _navMesh.isStopped = false;
        }

        public bool CanPerform => !Complete;
        public bool Complete => _gun.AmmoSystem.IsCurrentMagazineEmpty;



    }
}