using Attributes;
using EventBus;
using General;
using Items.Guns;
using Levels.Strategies;
using Npcs.Sensors;
using Npcs.Shared;
using Npcs.States;
using Npcs.States.Enemy;
using Player;
using UnityEngine;
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
        [SerializeField] [Required] private RaycastObjectSensor playerSensor;
        [SerializeField] [Required] private CoverPointSensor coverPointSensor;
        [SerializeField] private Gun gun;
        private CountdownTimer _damagedTimer;

        private Vector3 _lastDamageDirection;

        private bool _recentlyDamaged;
        private RigHandler _rigHandler;
        private Transform _transform;


        public Gun Gun => gun;
        public Transform AimPoint => aimPoint;
        public RaycastObjectSensor PlayerSensor => playerSensor;
        public CoverPointSensor CoverPointSensor => coverPointSensor;

        public float LookAtSpeed => lookAtSpeed;

        public float PointFollowSpeed => pointFollowSpeed;


        private void Start()
        {
            gun.Equip();
            _rigHandler = GetComponent<RigHandler>();

            _rigHandler.SetLeftHandData(Gun.leftHandTarget, Gun.leftHandHint);
            _transform = NavMeshAgent.transform;
        }


        protected override void SetUpTimers()
        {
            base.SetUpTimers();
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

            if (NavMeshAgent.hasPath && NavMeshAgent.remainingDistance > pathCompletionThreshold) return;

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

        public void LookAtDamageDirection()
        {
            LookAtTarget(playerSensor.Target.position, lookAtSpeed);
        }

        public void LookAtTarget(Vector3 target, float turnSpeed)
        {
            _transform.LookAt(target);
            _transform.rotation = Quaternion.Euler(0, _transform.rotation.eulerAngles.y, 0);
        }


        public override void TakeDamage(float amount, Vector3 direction, Vector3 damageLocation)
        {
            if (IsDead) return;

            base.TakeDamage(amount, direction, damageLocation);
            _lastDamageDirection = -direction;
            _damagedTimer.Start();
        }

        public override void Die()
        {
            base.Die();
            EventBus<EnemyKilledEvent>.Raise(new EnemyKilledEvent(this, transform.position));
        }
    }
}