using System.Collections;
using Attributes;
using General;
using NPC;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class Npc : MonoBehaviour, IDamageable, IKillable
{
    [SerializeField] private float health = 100;

    [SerializeField] private bool invulnerable;
    public bool Invulnerable => invulnerable;
    public float Health => health;

    protected StateMachine.StateMachine StateMachine;

    protected NavMeshAgent NavMeshAgent;

    protected RagdollController RagdollController;

    protected Animator Animator;

    protected bool CanWalk = true;

    [SerializeField] protected float minIdleTime = 2f;
    [SerializeField] protected float maxIdleTime = 5f;

    [SerializeField, Required] public AnimationController animationController;
    private Collider[] _colliders;
    private CountdownTimer _idleTimer;

    protected abstract void InitializeStateMachine();




    protected virtual void Awake()
    {
        StateMachine = new StateMachine.StateMachine();
        NavMeshAgent = GetComponent<NavMeshAgent>();
        Animator = GetComponentInChildren<Animator>();
        RagdollController = GetComponent<RagdollController>();
        SetUpTimers();
        InitializeStateMachine();
    }

    protected virtual void SetUpTimers()
    {
        _idleTimer = new CountdownTimer(Random.Range(minIdleTime, maxIdleTime));
        _idleTimer.OnTimerStart += () => { CanWalk = false; };
        _idleTimer.OnTimerStop += () => { CanWalk = true; };
    }

    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0) Die();
    }

    public virtual void Die()
    {
        RagdollController.EnableRagdoll(true);
        NavMeshAgent.enabled = false;
    }

    public void WalkToPoint(Vector3 point)
    {
        NavMeshAgent.SetDestination(point);
    }


    public bool MoveToRandomPositionAtDistance(float targetDistance, int maxAttempts)
    {
        if (!NavMeshAgent.isActiveAndEnabled) return false;
        var startPosition = transform.position;

        for (var i = 0; i < maxAttempts; i++)
        {
            // Generate random direction
            var randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0; // Keep on same Y level


            // Calculate target position
            var targetPosition = startPosition + randomDirection * targetDistance;

            // Check if position is on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
            {
                // Verify the actual distance is close to desired
                var actualDistance = Vector3.Distance(startPosition, hit.position);

                if (Mathf.Abs(actualDistance - targetDistance) < 0.5f)
                {
                    NavMeshAgent.SetDestination(hit.position);
                    return true;
                }
            }
        }

        return false;
    }

    public void HandleMovementBlendTree()
    {
        animationController.HandleMovementBlendTree(NavMeshAgent.velocity);
    }
    public void WalkToRandomPoint(float range)
    {
        if (!NavMeshAgent.isActiveAndEnabled) return;
        var randomDirection = Random.insideUnitSphere * range;
        randomDirection += transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, range, -1);
        NavMeshAgent.SetDestination(navHit.position);
    }

    public virtual void StopMoving()
    {
        if (!NavMeshAgent.isActiveAndEnabled) return;
        NavMeshAgent.SetDestination(transform.position);
    }

    public void IdleWaitBeforeMoving()
    {
        _idleTimer.InitialTime = Random.Range(minIdleTime, maxIdleTime);
        _idleTimer.Start();
    }



    protected virtual void Update()
    {
        StateMachine.Update();
        _idleTimer.Tick(Time.deltaTime);
    }

    protected virtual void FixedUpdate()
    {
        StateMachine.FixedUpdate();
    }


     

    public void TakeDamage(float amount, Vector3 direction)
    {
        health -= amount;
        if (health <= 0) Die();
    }
}