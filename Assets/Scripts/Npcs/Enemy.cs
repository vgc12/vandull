using System;
using Attributes;
using Environment;
using EventBus;
using General;
using Items.Guns;
using Levels.Strategies;
using Npcs.Sensors;
using Npcs.Shared;
using Npcs.States;
using Npcs.States.Enemy;
using Player;
using Reflex.Attributes;
using UnityEngine;
using ILogger = General.Logging.ILogger;
using Random = UnityEngine.Random;

namespace Npcs
{
    [RequireComponent(typeof(RigHandler))]
    public class Enemy : Npc
    {
        [Header("Movement Settings")] [SerializeField]
        private float strafeDistance = 8f;

        [SerializeField] private float closeRangeMultiplier = 0.7f;
        [SerializeField] private float farRangeMultiplier = 1.3f;
        [SerializeField] private float approachDistance = 5f;
        [SerializeField] private float pathCompletionThreshold = 2f;
        [SerializeField] private float engagementRange = 7f;
        [SerializeField] private float pointFollowSpeed = 5f;


        [Header("Combat Settings")] [SerializeField]
        private float damagedDuration = 4f;

        [SerializeField] private float lookAtSpeed = 10f;

        [SerializeField] [Required] private Transform aimPoint;
        [SerializeField] [Required] private LineOfSightSensor playerSensor;
        [SerializeField] [Required] private PatrolPointManager<WalkPoint> walkPatrolPointSensor;
        [SerializeField] [Required] private MultiTargetTypeSensor<Enemy> enemySensor;

        [SerializeField] private Gun gun;

        [SerializeField] private float onGuardDuration = 30f;

        [Inject] private readonly ILogger _logger;
        private CountdownTimer _guardTimer;
        private RigHandler _rigHandler;


        private bool _threatDetected;


        private CountdownTimer _threatTimer;
        private Transform _transform;
        public EventBinding<ThreatEvent> ThreatEventBinding;


        public Gun Gun => gun;
        public Transform AimPoint => aimPoint;
        public ISensor PlayerSensor => playerSensor;
        public PatrolPointManager<WalkPoint> WalkPatrolPointSensor => walkPatrolPointSensor;

        public float LookAtSpeed => lookAtSpeed;

        public float PointFollowSpeed => pointFollowSpeed;
        public bool PlayerDetected => playerSensor.CanSeeTarget;

        private bool OnGuard { get; set; }

        private Vector3 LastThreatPosition { get; set; }


        private void Start()
        {
            gun.Equip();
            _rigHandler = GetComponent<RigHandler>();
            ThreatEventBinding = new EventBinding<ThreatEvent>(OnThreatDetected);
            EventBus<ThreatEvent>.Register(ThreatEventBinding);

            name = "Enemy : " + Guid.NewGuid();
            _rigHandler.SetLeftHandData(Gun.leftHandTarget, Gun.leftHandHint);
            _rigHandler.LeftHandFollowItemHint = true;
            _rigHandler.LeftHandFollowItemTarget = true;
            _rigHandler.SetRightHandData(Gun.rightHandTarget, Gun.rightHandHint);
            _rigHandler.RightHandFollowItemHint = true;
            _rigHandler.RightHandFollowItemTarget = true;
            _transform = NavMeshAgent.transform;
        }


        protected override void Update()
        {
            base.Update();
            CheckOnGuardStatus();
        }

        private void OnDestroy() => EventBus<ThreatEvent>.Deregister(ThreatEventBinding);


        protected override void SetUpTimers()
        {
            base.SetUpTimers();
            _guardTimer = new CountdownTimer(onGuardDuration);
            _guardTimer.OnTimerStart += () =>
            {
                _logger.LogWarning($"{name} is now on guard!");
                playerSensor.DetectionMultiplier = 2f;
                OnGuard = true;
            };
            _guardTimer.OnTimerStop += () =>
            {
                OnGuard = false;
                playerSensor.DetectionMultiplier = 1f;
                _logger.LogWarning($"{name} is no longer on guard.");
            };
            Timers.Add(_guardTimer);
            _threatTimer = new CountdownTimer(damagedDuration);
            _threatTimer.OnTimerStart += () => _threatDetected = true;
            _threatTimer.OnTimerStop += () => _threatDetected = false;
            Timers.Add(_threatTimer);
        }

        protected override void InitializeStateMachine()
        {
            var idleState = new NpcIdleState(this);
            var wanderState = new EnemyWanderState(this);
            var attackState = new AttackPlayerState(this);
            var damagedState = new EnemyDamagedState(this);
            var deadState = new EnemyDeadState(this);

            StateMachine.AddAnyTransition(deadState, () => IsDead);
            StateMachine.AddAnyTransition(attackState, () => playerSensor.CanSeeTarget && !IsDead);
            StateMachine.AddAnyTransition(damagedState,
                () => !playerSensor.CanSeeTarget && _threatDetected && !IsDead);
            StateMachine.AddTransition(attackState, idleState,
                () => !playerSensor.CanSeeTarget && !NavMeshAgent.pathPending);
            StateMachine.AddTransition(attackState, wanderState,
                () => !playerSensor.CanSeeTarget && NavMeshAgent.pathPending && NavMeshAgent.remainingDistance <= 1f);
            StateMachine.AddTransition(wanderState,
                idleState,
                () => !CanWalk && !playerSensor.CanSeeTarget);
            StateMachine.AddTransition(idleState, wanderState,
                () => CanWalk && !playerSensor.CanSeeTarget);

            StateMachine.SetState(idleState);
        }

        public void HandleTacticalMovement()
        {
            var distanceToTarget = Vector3.Distance(_transform.position, playerSensor.Target.transform.position);

            if (NavMeshAgent.hasPath && NavMeshAgent.remainingDistance > pathCompletionThreshold)
            {
                return;
            }

            var strafePosition = GetStrafePosition(distanceToTarget);
            NavMeshAgent.SetDestination(strafePosition);
        }

        private Vector3 GetStrafePosition(float currentDistance)
        {
            var targetPos = playerSensor.Target.transform.position;
            var currentPos = _transform.position;

            if (currentDistance < engagementRange * closeRangeMultiplier)
            {
                var awayDirection = (currentPos - targetPos).normalized;
                return targetPos + awayDirection * engagementRange;
            }

            if (currentDistance > engagementRange * farRangeMultiplier)
            {
                var towardDirection = (targetPos - currentPos).normalized;
                return currentPos + towardDirection * approachDistance;
            }

            var toTarget = (targetPos - currentPos).normalized;
            var strafeDirection = GetRandomStrafeDirection(toTarget);
            return currentPos + strafeDirection * strafeDistance;
        }

        private static Vector3 GetRandomStrafeDirection(Vector3 toTarget)
        {
            var rightDirection = Vector3.Cross(toTarget, Vector3.up).normalized;
            return Random.value > 0.5f ? rightDirection : -rightDirection;
        }

        public void LookAtDamageDirection() => LookAtPoint(LastThreatPosition, lookAtSpeed);

        public void LookAtPoint(Vector3 targetPoint, float turnSpeed)
        {
            // Calculate direction from enemy to target point
            var direction = targetPoint - _transform.position;

            var targetRotation = Quaternion.LookRotation(direction);

            _transform.rotation = Quaternion.Slerp(
                _transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        public override void TakeDamage(float amount, Vector3 direction, Transform damageLocation)
        {
            if (IsDead)
            {
                return;
            }

            OnEnemyThreatened(damageLocation.position);

            base.TakeDamage(amount, direction, damageLocation);
        }

        private void OnEnemyThreatened(Vector3 position)
        {
            if (IsDead || _threatDetected) return;
            LastThreatPosition = position;
            _threatTimer.Start();
        }

        public void OnThreatDetected(ThreatEvent evt)
        {
            if (evt.Enemy != this) return;
            OnEnemyThreatened(evt.ThreatSourcePosition);
        }

        public override void Die()
        {
            base.Die();
            EventBus<EnemyKilledEvent>.Raise(new EnemyKilledEvent(this, transform.position));
        }

        public void CheckOnGuardStatus()
        {
            if (!enemySensor.CanSeeTarget || OnGuard || IsDead)
            {
                return;
            }

            playerSensor.DetectionMultiplier = 2f;

            _logger.LogWarning($"{name} is now on guard!");
            OnGuard = true;
        }

        public void StopSensors()
        {
            playerSensor.enabled = false;
            walkPatrolPointSensor.enabled = false;
            enemySensor.enabled = false;
        }
    }
}