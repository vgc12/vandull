using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using General;
using NPC.Strategies;
using Player;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace NPC.GOAP
{
    public abstract class GoapAgent : MonoBehaviour, IDamageable, IKillable
    {
        [Header("Sensors")] [SerializeField] protected CoverPointSensor coverPointSensor;
        [SerializeField] protected EnemyObjectSensor playerRaycastSensor;

        [Header("Configuration")] [SerializeField]
        private float damageTurnCooldown = 5f;

        // Components
        private RagdollController _ragdollController;
        protected NavMeshAgent NavMeshAgent { get; private set; }
        protected Rigidbody Rigidbody { get; private set; }
        
        protected AnimationController AnimationController;

        // Health & State
        [SerializeField] private bool invulnerable;
        public bool Invulnerable => invulnerable;
        public float Health { get; private set; }

        // GOAP System
        public ActionPlan ActionPlan { get; private set; }
        public AgentGoal CurrentGoal;
        public AgentAction CurrentAction;
        public Dictionary<BeliefType, AgentBelief> Beliefs;
        public HashSet<AgentAction> Actions;
        public HashSet<AgentGoal> Goals;

        private IGoapPlanner _goapPlanner;
        protected BeliefFactory Factory;
        private AgentGoal _lastGoal;

        // Timers
        private CountdownTimer _statsTimer;
        private CountdownTimer _damageTurnTimer;
        private CountdownTimer _damagedRecentlyTimer;
        
        // Movement & Targeting
        private GameObject _target;
        private Vector3 _destination;
        private bool _canTurn = true;
        public bool DamagedRecently => _damagedRecently;
        private bool _damagedRecently = false;


        public virtual void Awake()
        {
            AnimationController = GetComponentInChildren<AnimationController>();
            NavMeshAgent = GetComponent<NavMeshAgent>();

            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.freezeRotation = true;
            _goapPlanner = new GoapPlanner();
            _ragdollController = GetComponent<RagdollController>();
            _ragdollController.DisableRagdoll();
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
                    .AddPrecondition(Beliefs[BeliefType.IsSafe])
                    .AddPrecondition(Beliefs[BeliefType.NotDamagedRecently])
                    .WithStrategy(new IdleStrategy(5))
                    .WithCost(5)
                    .AddEffect(Beliefs[BeliefType.Nothing])
                    .Build(),
                new AgentAction.Builder("Wander Around")
                    .AddPrecondition(Beliefs[BeliefType.NotDamagedRecently])
                    .AddPrecondition(Beliefs[BeliefType.IsSafe])
                    .WithStrategy(new MoveStrategy(NavMeshAgent, 10))
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
            Factory.AddBelief(BeliefType.IsSafe,
                () => !playerRaycastSensor.canSeeTarget || Mathf.Approximately(Health, 100));
            Factory.AddBelief(BeliefType.IsNotSafe, () => playerRaycastSensor.canSeeTarget && Health <= 30);
            Factory.AddBelief(BeliefType.JustTookDamage, () => _damagedRecently);
         
            Factory.AddBelief(BeliefType.NotDamagedRecently, () => !_damagedRecently);
            Factory.AddBelief(BeliefType.CanSeePlayer, () => playerRaycastSensor.canSeeTarget);
            
            Factory.AddBelief(BeliefType.CanNotSeePlayer, () => !playerRaycastSensor.CanSeeTarget);
        }

        private void SetupTimers()
        {
            _statsTimer = new CountdownTimer(2f);
            _statsTimer.OnTimerStop += () =>
            {
                UpdateStats();
                _statsTimer.Start();
            };

            _damageTurnTimer = new CountdownTimer(damageTurnCooldown);
            _damageTurnTimer.OnTimerStop += () => { _canTurn = true; };
            
            _damagedRecentlyTimer = new CountdownTimer(15f);
            _damagedRecentlyTimer.OnTimerStart += () => { _damagedRecently = true; };
            _damagedRecentlyTimer.OnTimerStop += () => { _damagedRecently = false; };

            _statsTimer.Start();
        }

        protected virtual void UpdateStats()
        {
        }

        protected bool InRangeOf(Vector3 position, float range) =>
            Vector3.Distance(transform.position, position) < range;


        public void TakeDamage(float amount, Vector3 direction)
        {
            if (Invulnerable) return;

            _damagedRecentlyTimer.Start();
            Health -= amount;
            if (direction != Vector3.zero && _canTurn && Beliefs[BeliefType.IsSafe].Evaluate())
            {
                
                ResetGoal();
                _canTurn = false;
                _damageTurnTimer.Start();
                StartCoroutine(TurnToDamage(-direction));
            }

            VandullLogger.Log($"{gameObject.name} took {amount} damage, health now at {Health}");
            if (Health <= 0)
            {
                Die();
            }
        }

        protected void ResetGoal()
        {
            CurrentAction = null;
            CurrentGoal = null;
        }


        private IEnumerator TurnToDamage(Vector3 direction)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * .2f;
                NavMeshAgent.transform.rotation = Quaternion.Slerp(NavMeshAgent.transform.rotation,
                    Quaternion.LookRotation(direction), t);
                NavMeshAgent.transform.rotation = Quaternion.Euler(0, NavMeshAgent.transform.rotation.eulerAngles.y, 0);
                yield return null;
            }
        }


        public virtual void Die()
        {
            NavMeshAgent.enabled = false;
            CurrentAction = null;
            CurrentGoal = null;
            _ragdollController.EnableRagdoll();
        }


        protected virtual void Update()
        {
            _statsTimer.Tick(Time.deltaTime);
            AnimationController.HandleMovementBlendTree(NavMeshAgent.velocity);

            // Update the plan and current action if there is one
            if (CurrentAction == null)
            {
//                VandullLogger.Log("Calculating any potential new plan");
                CalculatePlan();

                if (ActionPlan != null && ActionPlan.Actions.Count > 0)
                {
                    if (NavMeshAgent.enabled)
                    {
                        NavMeshAgent.ResetPath();
                    }

                    CurrentGoal = ActionPlan.AgentGoal;
                //    VandullLogger.Log($"Goal: {CurrentGoal.Name} with {ActionPlan.Actions.Count} actions in plan");
                    CurrentAction = ActionPlan.Actions.Pop();
               //     VandullLogger.Log($"Popped action: {CurrentAction.Name}");
                    // Verify all precondition effects are true
                    if (CurrentAction.Preconditions.All(b => b.Evaluate()))
                    {
                        CurrentAction.Start();
                    }
                    else
                    {
             //           VandullLogger.Log("Preconditions not met, clearing current action and goal");
                        CurrentAction = null;
                        CurrentGoal = null;
                    }
                }
            }

            // If we have a current action, execute it
            if (ActionPlan != null && CurrentAction != null)
            {
                CurrentAction.Update(Time.deltaTime);

                if (CurrentAction.Complete)
                {
                    VandullLogger.Log($"{CurrentAction.Name} complete");
                    CurrentAction.Stop();
                    CurrentAction = null;

                    if (ActionPlan.Actions.Count == 0)
                    {
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
            playerRaycastSensor.OnTargetSpotted += HandleTargetFound;
            playerRaycastSensor.OnTargetLost += HandleTargetLost;
        }

        protected virtual void HandleTargetLost()
        {
            
        }

        protected virtual void HandleTargetFound()
        {
            
        }

  
        private void OnDisable()
        {
            if (playerRaycastSensor == null) return;
          
            playerRaycastSensor.OnTargetLost -= HandleTargetLost;
            playerRaycastSensor.OnTargetSpotted -= HandleTargetFound;
        }
    }
}