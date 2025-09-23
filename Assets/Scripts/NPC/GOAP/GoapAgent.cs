using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using General;
using NPC;
using NPC.GOAP;
using UnityEngine;
using UnityEngine.AI;

public class GoapAgent : MonoBehaviour, IDamageable, IKillable
{
    [Header("Sensors")] [SerializeField] private Sensor chaseSensor;
    [SerializeField] private Sensor attackSensor;

    [Header("Known Locations")] [SerializeField]
    private Transform restingPosition;


    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private Rigidbody _rigidbody;

    public float Health { get; private set; }
    public ActionPlan ActionPlan { get; private set; }

    private CountdownTimer _statsTimer;

    private GameObject _target;

    private Vector3 _destination;

    private AgentGoal _lastGoal;

    public AgentGoal CurrentGoal;

    //public ActionPlan currentPlan;
    public AgentAction CurrentAction;

    public Dictionary<string, AgentBelief> Beliefs;
    public HashSet<AgentAction> Actions;
    public HashSet<AgentGoal> Goals;

    private IGoapPlanner _goapPlanner;

    private AnimationController _animationController;
    
    private void Awake()
    {
        _animationController = GetComponentInChildren<AnimationController>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
        _goapPlanner = new GoapPlanner();
    }

    private void Start()
    {
        SetupTimers();
        SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    private void SetupActions()
    {
        Actions = new HashSet<AgentAction>();
        Actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy( 5))
            .AddEffect(Beliefs["Nothing"])
            .Build());
        Actions.Add(new AgentAction.Builder("Wander Around")
            .WithStrategy(new WanderStrategy(_navMeshAgent, 10))
            .AddEffect(Beliefs["AgentMoving"]).Build());
    }

    private void SetupGoals()
    {
        Goals = new HashSet<AgentGoal>();
        Goals.Add(
            new AgentGoal.Builder("Chill Out")
                .WithPriority(1)
                .WithDesiredEffect(Beliefs["Nothing"])
                .Build()
        );

        Goals.Add(
            new AgentGoal.Builder("Wander")
                .WithPriority(1)
                .WithDesiredEffect(Beliefs["AgentMoving"])
                .Build()
        );
    }

    private void SetupBeliefs()
    {
        Beliefs = new Dictionary<string, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, Beliefs);
        factory.AddBelief("Nothing", () => false);
        factory.AddBelief("AgentIdle", () => !_navMeshAgent.hasPath);
        factory.AddBelief("AgentMoving", () => _navMeshAgent.hasPath);
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

    private void UpdateStats()
    {
    }

    bool InRangeOf(Vector3 position, float range) => Vector3.Distance(transform.position, position) < range;

    public void TakeDamage(float amount)
    {
        Health -= amount;
    }


    public void Die()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        chaseSensor.OnTargetChanged += HandleTargetChanged;
    }

    private void OnDisable()
    {
        chaseSensor.OnTargetChanged -= HandleTargetChanged;
    }

    private void HandleTargetChanged()
    {
        VandullLogger.Log("Target changged, clearing current action and goal");
        CurrentGoal = null;
        CurrentAction = null;
    }

    void Update() {
        _statsTimer.Tick(Time.deltaTime);
       _animationController.HandleMovementBlendTree(_navMeshAgent.velocity);
        
        // Update the plan and current action if there is one
        if (CurrentAction == null) {
            VandullLogger.Log("Calculating any potential new plan");
            CalculatePlan();

            if (ActionPlan != null && ActionPlan.Actions.Count > 0) {
                _navMeshAgent.ResetPath();

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
}