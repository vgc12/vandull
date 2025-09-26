using System;
using System.Collections.Generic;
using System.Linq;
using General;
using NPC.Strategies;
using Player;
using UnityEngine;
using UnityEngine.AI;

namespace NPC.GOAP
{
    public abstract class GoapAgent : MonoBehaviour, IDamageable, IKillable
    {
        [Header("Sensors")] 
        [SerializeField] protected CoverPointSensor coverPointSensor;
        [SerializeField] protected PlayerSensor playerRaycastSensor;
        
        protected NavMeshAgent NavMeshAgent { get; private set; }
  
        protected Rigidbody Rigidbody { get; private set; }

        public float Health { get; private set; }
        public ActionPlan ActionPlan { get; private set; }

        private CountdownTimer _statsTimer;

        private GameObject _target;

        private Vector3 _destination;

        private AgentGoal _lastGoal;

        public AgentGoal CurrentGoal;

        //public ActionPlan currentPlan;
        public AgentAction CurrentAction;

        public Dictionary<BeliefType, AgentBelief> Beliefs;
        public HashSet<AgentAction> Actions;
        public HashSet<AgentGoal> Goals;

        private IGoapPlanner _goapPlanner;

        protected AnimationController AnimationController;
        
        protected BeliefFactory Factory;
    
        public virtual void Awake()
        {
            AnimationController = GetComponentInChildren<AnimationController>();
            NavMeshAgent = GetComponent<NavMeshAgent>();
          
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.freezeRotation = true;
            _goapPlanner = new GoapPlanner();
            
            Health = 100;
        }

        protected virtual void Start()
        {
            SetupTimers();
            SetupBeliefs();
            SetupActions();
            SetupGoals();
        }

        protected virtual void SetupActions()
        {
            Actions = new HashSet<AgentAction>
            {
                
                new AgentAction.Builder("Idle")
                    .WithStrategy(new IdleStrategy( 5))
                    .AddEffect(Beliefs[BeliefType.Nothing])
                    .Build(),
                new AgentAction.Builder("Wander Around")
                    .WithStrategy(new WanderStrategy(NavMeshAgent, 10))
                    .AddEffect(Beliefs[BeliefType.AgentMoving])
                    .Build()
                    
            };
        }

        protected virtual void SetupGoals()
        {
            Goals = new HashSet<AgentGoal>
            {
                new AgentGoal.Builder("Idle")
                    .WithPriority(1)
                    .WithDesiredEffect(Beliefs[BeliefType.Nothing])
                    .Build(),
                new AgentGoal.Builder("Wander")
                    .WithPriority(1)
                    .WithDesiredEffect(Beliefs[BeliefType.AgentMoving])
                    .Build(),
                new AgentGoal.Builder("Stay Alive")
                    .WithPriority(2)
                    .WithDesiredEffect(Beliefs[BeliefType.IsSafe])
                    .Build()
            };
        }

        protected virtual void SetupBeliefs()
        {
            Beliefs = new Dictionary<BeliefType, AgentBelief>();
            Factory = new BeliefFactory(this, Beliefs);
            Factory.AddBelief(BeliefType.Nothing, () => false);
            Factory.AddBelief(BeliefType.AgentIdle, () => !NavMeshAgent.hasPath);
            Factory.AddBelief(BeliefType.AgentMoving, () => NavMeshAgent.hasPath);
            Factory.AddBelief(BeliefType.HealthLow, () => Health <= 30);
            Factory.AddBelief(BeliefType.HealthFine, () => Health >= 75);
            Factory.AddBelief(BeliefType.IsSafe, () => !playerRaycastSensor.IsTargetPresent || Health >= 75);
            Factory.AddBelief(BeliefType.IsNotSafe, () => playerRaycastSensor.IsTargetPresent && Health <= 30);
            Factory.AddSensorBelief(BeliefType.CoverInRange, coverPointSensor);
        }

        private void SetupTimers()
        {
            _statsTimer = new CountdownTimer(2f);
            _statsTimer.OnTimerStop += () =>
            {
                UpdateStats();
                _statsTimer.Start();
            };
            _statsTimer.Start();
        }

        protected virtual void UpdateStats()
        {
            
        }

        protected bool InRangeOf(Vector3 position, float range) => Vector3.Distance(transform.position, position) < range;
        
        private bool findEmergencyCover;

        public void TakeDamage(float amount)
        {
            Health -= amount;

            if (Health <= 35 && !findEmergencyCover)
            {
                findEmergencyCover = true;
                CurrentAction = null;
                CurrentGoal = null;
            }
            
            VandullLogger.Log($"{gameObject.name} took {amount} damage, health now at {Health}");
            if (Health <= 0)
            {
                Die();
            }
        }


        public void Die()
        {
            gameObject.SetActive(false);
        }

        protected virtual void HandleTargetChanged()
        {
            VandullLogger.Log("Target changged, clearing current action and goal");
            CurrentGoal = null;
            CurrentAction = null;
        }

       protected virtual void Update() {
           
            _statsTimer.Tick(Time.deltaTime);
            AnimationController.HandleMovementBlendTree(NavMeshAgent.velocity);
        
            // Update the plan and current action if there is one
            if (CurrentAction == null) {
                VandullLogger.Log("Calculating any potential new plan");
                CalculatePlan();

                if (ActionPlan != null && ActionPlan.Actions.Count > 0) {
                    NavMeshAgent.ResetPath();

                    CurrentGoal = ActionPlan.AgentGoal;
                    VandullLogger.Log($"Goal: {CurrentGoal.Name} with {ActionPlan.Actions.Count} actions in plan");
                    CurrentAction = ActionPlan.Actions.Pop();
                    VandullLogger.Log($"Popped action: {CurrentAction.Name}");
                    // Verify all precondition effects are true
                    if (CurrentAction.Preconditions.All(b => b.Evaluate())) {
                        CurrentAction.Start();
                    } else {
                        VandullLogger.Log("Preconditions not met, clearing current action and goal");
                        CurrentAction = null;
                        CurrentGoal = null;
                    }
                }
            }

            // If we have a current action, execute it
            if (ActionPlan != null && CurrentAction != null) {
                CurrentAction.Update(Time.deltaTime);

                if (CurrentAction.Complete) {
                    VandullLogger.Log($"{CurrentAction.Name} complete");
                    CurrentAction.Stop();
                    CurrentAction = null;

                    if (ActionPlan.Actions.Count == 0) {
                        VandullLogger.Log("Plan complete");
                        _lastGoal = CurrentGoal;
                        CurrentGoal = null;
                    }
                }
            }
        }

        private void CalculatePlan()
        {
            var priorityLevel = CurrentGoal?.Priority ?? 0;

            HashSet<AgentGoal> goalsToCheck = Goals;
        
            // If there is a current goal we only want to check goals with a higher priority

            if (CurrentGoal != null)
            {
                VandullLogger.Log("Current goal exists, checking goals with a higher priority");
                goalsToCheck = new HashSet<AgentGoal>(Goals.Where(g => g.Priority > priorityLevel));
            }
        
            var potentialPlan = _goapPlanner.Plan(this, goalsToCheck, _lastGoal);

            if (potentialPlan != null)
            {
                ActionPlan = potentialPlan;
            }

        }

        private void OnDrawGizmos()
        {
            if (Beliefs == null) return;
            if (Beliefs.TryGetValue(BeliefType.CoverInRange, out AgentBelief belief))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(belief.Location, 0.3f);
            }
            Gizmos.DrawSphere(NavMeshAgent.destination, 1f);
        }

        private void OnEnable()
        {
            if (coverPointSensor != null)
            {
               // coverPointSensor.OnTargetChanged += HandleTargetChanged;
            }
        }
        
        private void OnDisable()
        {
            if (coverPointSensor != null)
            {
              //  coverPointSensor.OnTargetChanged -= HandleTargetChanged;
            }
        }
    }


}