using System;
using General;
using Items.Guns;
using NPC.GOAP;
using UnityEngine;
using UnityEngine.AI;

namespace NPC
{
    
       public class CombatEngageStrategy : IActionStrategy
    {
        private readonly Gun _gun;
        private readonly EnemyObjectSensor _sensor;
        private readonly Transform _aimPoint;
        private readonly float _engagementRange;
        private readonly CountdownTimer _combatTimer;
        private readonly NavMeshAgent _navMesh;
        public CombatEngageStrategy(NavMeshAgent navMesh, Gun gun, EnemyObjectSensor sensor, 
            Transform aimPoint,Func<bool> damagedRecently, float engagementRange = 15f, float combatDuration = 8f)
        {
            _navMesh = navMesh;
            _gun = gun;
            _sensor = sensor;
            _aimPoint = aimPoint;
            _engagementRange = engagementRange;
            _combatTimer = new CountdownTimer(combatDuration);
            _combatTimer.OnTimerStop += () =>
            {
                if (damagedRecently() && !_sensor.CanSeeTarget)
                {
                    Complete = true;
                    return;
                }
                _combatTimer.Start();
            };
        }

        public bool CanPerform => _sensor.CanSeeTarget && !_gun.AmmoSystem.CurrentMagazineEmpty;
        
        public bool Complete { get; private set; }

        public  void Start()
        {
            _gun.FireModeSystem.SetCurrentFireMode(FireType.Automatic);
            _gun.StartAutomaticFire();
            _combatTimer.Start();
        }

        public void Update(float deltaTime)
        {
            _combatTimer.Tick(deltaTime);

            if (!_sensor.CanSeeTarget)
            {
                _gun.StopAutomaticFire();
                Complete = true;
                return;
            }

            // Aim while moving
            _aimPoint.position = _sensor.Target.transform.position;
            LookAtTarget(_sensor.Target.transform.position, deltaTime);

            // Move tactically while shooting
            HandleTacticalMovement();

            // Check if we need to reload
            if (_gun.AmmoSystem.CurrentMagazineEmpty)
            {
                _gun.StopAutomaticFire();
                Complete = true; // Let GOAP plan reload action
            }
            else
            {
                _gun.AmmoSystem.StartReload();
            }
        }

        private void HandleTacticalMovement()
        {
            float distanceToTarget = Vector3.Distance(_navMesh.transform.position, _sensor.Target.transform.position);
            

            if (_navMesh.remainingDistance <= 2f || !_navMesh.hasPath)
            {
                Vector3 strafePosition = GetStrafePosition(distanceToTarget);
                _navMesh.SetDestination(strafePosition);
            }
        }

        private Vector3 GetStrafePosition(float currentDistance)
        {
            Vector3 targetPos = _sensor.Target.transform.position;
            Vector3 currentPos = _navMesh.transform.position;
            
       
            if (currentDistance < _engagementRange * 0.7f)
            {
                Vector3 awayDirection = (currentPos - targetPos).normalized;
                return targetPos + awayDirection * _engagementRange;
            }
   

            if (currentDistance > _engagementRange * 1.3f)
            {
                Vector3 towardDirection = (targetPos - currentPos).normalized;
                return currentPos + towardDirection * 5f;
            }
            // Perfect distance - strafe sideways

            Vector3 toTarget = (targetPos - currentPos).normalized;
            Vector3 rightDirection = Vector3.Cross(toTarget, Vector3.up).normalized;
                
            // Randomly choose left or right strafe
            Vector3 strafeDirection = UnityEngine.Random.value > 0.5f ? rightDirection : -rightDirection;
            return currentPos + strafeDirection * 8f;
        }

        private void LookAtTarget(Vector3 target, float deltaTime)
        {
            Vector3 direction = target - _gun.FireModeSystem.CurrentFireSystem.MuzzleTransform.position;
            _navMesh.transform.rotation = Quaternion.Slerp(_navMesh.transform.rotation,
                Quaternion.LookRotation(direction), deltaTime * 10f);
            _navMesh.transform.rotation = Quaternion.Euler(0, _navMesh.transform.rotation.eulerAngles.y, 0);
        }

        public void Stop()
        {
            _gun.StopAutomaticFire();
        }
    }

    
    public class ShootPlayerStrategy : MoveStrategy
    {
        private readonly Gun _gun;
        private readonly NavMeshAgent _navMesh;
        private readonly AnimationController _animationController;
        private readonly Transform _aimPoint;

        private readonly EnemyObjectSensor _sensor;

        private readonly Func<bool> _damagedRecently;

        private readonly CountdownTimer _endStrategyTimer;

        public ShootPlayerStrategy(NavMeshAgent navMesh, AnimationController animationController, Gun gun,
            Transform aimPoint, EnemyObjectSensor sensor, Func<bool> damagedRecently) : base(navMesh)
        {
            _navMesh = navMesh;
            _gun = gun;
            _sensor = sensor;
            _animationController = animationController;
            _aimPoint = aimPoint;
            _damagedRecently = damagedRecently;
            _endStrategyTimer = new CountdownTimer(8f);
            _endStrategyTimer.OnTimerStop += () =>
            {
                if (_damagedRecently())
                {
                    _endStrategyTimer.Start();
                }
                else
                {
                    Complete = true;
                }
            };

        }

        public override void Start()
        {
            _endStrategyTimer.Start();
            VandullLogger.LogWarning("Starting to shoot at player");
            _gun.FireModeSystem.SetCurrentFireMode(FireType.Automatic);
            _gun.StartAutomaticFire();
        }


        public override void Update(float deltaTime)
        {
         
            if (!_sensor.CanSeeTarget)
            {
                
                _gun.StopAutomaticFire();
                LookAtTarget(_sensor.LastKnownPosition , deltaTime);
                MoveToLastPosition();   
                return;
            }
            _aimPoint.position = _sensor.Target.transform.position;
            MoveRandomly();

            LookAtTarget(_sensor.Target.transform.position, deltaTime);

            if (_gun.AmmoSystem.CurrentMagazineEmpty)
            {
                _gun.StartReload();
            }
        }

        private void MoveToLastPosition()
        {
            var destination =_sensor.LastKnownPosition;
            if (destination == Vector3.zero)
            {
                VandullLogger.LogWarning("MoveToLastPosition: Nowhere to move");
                MoveRandomly(); 
                return;
            }
            VandullLogger.LogWarning("MoveToLastPosition: Moving to last position");
            Debug.DrawLine(_navMesh.transform.position, destination, Color.white);
            _navMesh.SetDestination(destination);
        }

        private void LookAtTarget( Vector3 target,float deltaTime)
        {
            
            var direction = target - _gun.FireModeSystem.CurrentFireSystem.MuzzleTransform.position;
            _navMesh.transform.rotation = Quaternion.Slerp(_navMesh.transform.rotation,
                Quaternion.LookRotation(direction), deltaTime * 10f);
            _navMesh.transform.rotation = Quaternion.Euler(0, _navMesh.transform.rotation.eulerAngles.y, 0);
    
        }


        public void MoveRandomly()
        {
         if(_navMesh.remainingDistance <= 2f ||!_navMesh.hasPath)
                MoveToRandomPositionAtDistance(10f, 30);
            
        }
        
        public override void Stop()
        {
            VandullLogger.LogWarning("Stopping shooting at player");
            _gun.StopAutomaticFire();
            _navMesh.isStopped = false;
        }

        public override bool CanPerform => true;
        public override bool Complete { get; protected set; }
    }
}