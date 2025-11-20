using System.Collections.Generic;
using Attributes;
using General;
using Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Npcs.Shared
{
    [RequireComponent(typeof(NavMeshAgent), typeof(RagdollController))]
    public abstract class Npc : MonoBehaviour, IDamageable, IKillable
    {
        #region Animation

        public void HandleMovementBlendTree()
        {
            animationController.HandleMovementBlendTree(NavMeshAgent.velocity);
        }

        #endregion

        #region Serialized Fields

        [SerializeField] private float health = 100;
        [SerializeField] private bool invulnerable;
        [SerializeField] protected float minIdleTime = 2f;
        [SerializeField] protected float maxIdleTime = 5f;
        [SerializeField] [Required] public AnimationController animationController;

        #endregion

        #region Properties

        public bool Invulnerable => invulnerable;

        public float Health
        {
            get => health;
            set => health = Mathf.Clamp(value, 0, maxHealth);
        }

        [SerializeField] private float maxHealth = 100f;

        public float MaxHealth => maxHealth;
        public bool IsDead => health <= 0;

        #endregion

        #region Protected Fields

        protected StateMachine.StateMachine StateMachine;
        public NavMeshAgent NavMeshAgent { get; private set; }
        protected List<Timer> Timers { get; private set; }
        protected RagdollController RagdollController;
        protected bool CanWalk = true;

        #endregion

        #region Private Fields

        private Collider[] _colliders;
        private CountdownTimer _idleTimer;

        #endregion

        #region Unity Lifecycle Methods

        protected virtual void Awake()
        {
            StateMachine = new StateMachine.StateMachine();
            NavMeshAgent = GetComponent<NavMeshAgent>();
            RagdollController = GetComponent<RagdollController>();
            Timers = new List<Timer>(10);
            SetUpTimers();
            InitializeStateMachine();
        }

        protected virtual void Update()
        {
            StateMachine.Update();
            foreach (var t in Timers) t.Tick(Time.deltaTime);
        }

        protected virtual void FixedUpdate()
        {
            StateMachine.FixedUpdate();
        }

        #endregion

        #region Initialization

        protected abstract void InitializeStateMachine();

        protected virtual void SetUpTimers()
        {
            _idleTimer = new CountdownTimer(Random.Range(minIdleTime, maxIdleTime));
            _idleTimer.OnTimerStart += OnTimerStart;
            _idleTimer.OnTimerStop += OnTimerStop;
            Timers.Add(_idleTimer);
        }

        private void OnTimerStart()
        {
            CanWalk = false;
        }

        private void OnTimerStop()
        {
            CanWalk = true;
        }

        #endregion

        #region Health & Damage

        public virtual void TakeDamage(float amount, Vector3 direction, Transform damageLocation)
        {
            if (invulnerable || IsDead) return;

            health -= amount;
            health = Mathf.Clamp(health, 0, float.MaxValue);
            if (IsDead) Die();
        }

        public virtual void Die()
        {
            RagdollController.EnableRagdoll(true);
            NavMeshAgent.enabled = false;
        }

        #endregion

        #region Movement Methods

        public void WalkToPoint(Vector3 point)
        {
            NavMeshAgent.SetDestination(point);
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
                if (!NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas)) continue;

                // Verify the actual distance is close to desired
                var actualDistance = Vector3.Distance(startPosition, hit.position);

                if (!(Mathf.Abs(actualDistance - targetDistance) < 0.5f)) continue;

                NavMeshAgent.SetDestination(hit.position);
                return true;
            }

            return false;
        }

        public virtual void StopMoving()
        {
            if (!NavMeshAgent.isActiveAndEnabled) return;
            
            CanWalk = false;

            NavMeshAgent.SetDestination(transform.position);
        }

        public void IdleWaitBeforeMoving()
        {
            _idleTimer.Reset();
            _idleTimer.InitialTime = Random.Range(minIdleTime, maxIdleTime);
            _idleTimer.Start();
        }

        #endregion
    }
}