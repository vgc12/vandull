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
using UnityEditor;
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


        private CountdownTimer _damagedTimer;
        private CountdownTimer _guardTimer;

        private Vector3 _lastDamageDirection;

        private bool _recentlyDamaged;
        private RigHandler _rigHandler;
        private Transform _transform;


        public Gun Gun => gun;
        public Transform AimPoint => aimPoint;
        public ISensor PlayerSensor => playerSensor;
        public PatrolPointManager<WalkPoint> WalkPatrolPointSensor => walkPatrolPointSensor;

        public float LookAtSpeed => lookAtSpeed;

        public float PointFollowSpeed => pointFollowSpeed;
        public bool PlayerDetected => playerSensor.CanSeeTarget;

        public bool OnGuard { get; private set; }


        private void Start()
        {
            gun.Equip();
            _rigHandler = GetComponent<RigHandler>();

            name = "Enemy : " + GUID.Generate();
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
            _damagedTimer = new CountdownTimer(damagedDuration);
            _damagedTimer.OnTimerStart += () => _recentlyDamaged = true;
            _damagedTimer.OnTimerStop += () => _recentlyDamaged = false;
            Timers.Add(_damagedTimer);
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
                () => !playerSensor.CanSeeTarget && _recentlyDamaged && !IsDead);
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

        public void LookAtDamageDirection() => LookAtTarget(playerSensor.Target.position, lookAtSpeed);

        public void LookAtTarget(Vector3 target, float turnSpeed)
        {
            _transform.LookAt(target);
            _transform.rotation = Quaternion.Euler(0, _transform.rotation.eulerAngles.y, 0);
        }


        public override void TakeDamage(float amount, Vector3 direction, Transform damageLocation)
        {
            if (IsDead)
            {
                return;
            }

            base.TakeDamage(amount, direction, damageLocation);
            _lastDamageDirection = -direction;
            _damagedTimer.Start();
        }

        public override void Die()
        {
            base.Die();
            EventBus<EnemyKilledEvent>.Raise(new EnemyKilledEvent(this, transform.position));
        }

        public void CheckOnGuardStatus()
        {
            _logger.Log(enemySensor.CanSeeTarget);
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