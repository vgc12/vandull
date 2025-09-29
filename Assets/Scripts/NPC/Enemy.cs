using Attributes;
using Items.Guns;
using Items.Guns.Firing;
using Items.Guns.Items.Guns.Dependencies;
using NPC.GOAP;
using Player;
using UnityEngine;

namespace NPC
{
    public class Enemy : GoapAgent
    {
        [SerializeField, Required] private Gun gun;
        [SerializeField, Required] private Transform aimPoint;


        protected override void SetupActions()
        {
            base.SetupActions();

            Actions.Add(new AgentAction.Builder("Flee")
                .AddPrecondition(Beliefs[BeliefType.IsNotSafe])
                .WithStrategy(new FleeStrategy(NavMeshAgent,
                    coverPointSensor, playerRaycastSensor))
                .AddEffect(Beliefs[BeliefType.IsSafe])
                .WithCost(3)
                .Build());

            Actions.Add(new AgentAction.Builder("Shoot Player")
                .AddPrecondition(Beliefs[BeliefType.CanSeePlayer])
                .AddPrecondition(Beliefs[BeliefType.HasAmmo])
                .AddPrecondition(Beliefs[BeliefType.PlayerAlive])/*
                .WithStrategy(new ShootPlayerStrategy(NavMeshAgent, AnimationController, gun, aimPoint,
                     playerRaycastSensor, () => DamagedRecently))*/
                .WithStrategy(new CombatEngageStrategy(NavMeshAgent, gun, playerRaycastSensor, aimPoint, () => DamagedRecently,10))
                .AddEffect(Beliefs[BeliefType.IsSafe])
                .AddEffect(Beliefs[BeliefType.PlayerDead])
                .AddEffect(Beliefs[BeliefType.HasNoAmmo])
                .Build());

            Actions.Add(new AgentAction.Builder("Reload Gun")
                .AddPrecondition(Beliefs[BeliefType.HasNoAmmo])
                .WithStrategy(new ReloadStrategy(gun)).AddEffect(Beliefs[BeliefType.HasAmmo]).Build());
        }

        protected override void SetupGoals()
        {
            base.SetupGoals();

            Goals.Add(new AgentGoal.Builder("Kill Player")
                .WithDesiredEffect(Beliefs[BeliefType.PlayerDead])
                .WithPriority(3)
                .Build());
        }

        protected override void HandleTargetFound()
        {
            if (CurrentAction?.Strategy?.GetType() == typeof(ShootPlayerStrategy))
            {
                return;
            }
            
            ResetGoal();

        }

       
        

        protected override void SetupBeliefs()
        {
            base.SetupBeliefs();
            Factory.AddBelief(BeliefType.HasAmmo, () => !gun.AmmoSystem.CurrentMagazineEmpty);
            Factory.AddBelief(BeliefType.HasNoAmmo, () => !gun.AmmoSystem.CurrentMagazineEmpty);

            Factory.AddBelief(BeliefType.PlayerDead, () =>
            {
                var killable = playerRaycastSensor.Target.GetComponentInParent<PlayerStateMachine>();
                return killable is { Health: <= 0 };
            });
            Factory.AddBelief(BeliefType.PlayerAlive, () =>
            {
                var killable = playerRaycastSensor.Target.GetComponentInParent<PlayerStateMachine>();
                return killable is { Health: > 0 };
            });
        }

        public override void Die()
        {
            base.Die();
            gun.Drop();
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
}