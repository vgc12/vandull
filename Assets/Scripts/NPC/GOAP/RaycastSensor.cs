using System;
using Attributes;
using General;
using Player;
using UnityEngine;

namespace NPC.GOAP
{
    public class PlayerSensor : RaycastSensor<PlayerStateMachine>
    {
        
    }
    public abstract class RaycastSensor <T> : MonoBehaviour, ISensor<T> where T : Component
    {
        [SerializeField] private float timerInterval = 1f;
        [SerializeField] private LayerMask detectionLayer;
        [SerializeField] private LayerMask obstructionLayer;
        [SerializeField] private Transform rayOrigin;
 
        private SphereCollider _detectionRange;
        public event Action OnTargetChanged = delegate { };
    
        public T TargetComponent { get; private set; }
        public Vector3 TargetPosition => _foundTarget ? _foundTarget.transform.position : Vector3.zero;
        public bool IsTargetPresent { get; private set; }

        // The currently detected target
        private GameObject _foundTarget;

        // The object that the sensor is trying to detect
        [Required, SerializeField] private GameObject sensorTarget;
 
        private Vector3 _lastKnownPosition;

        private CountdownTimer _timer;
    
        [SerializeField] private float detectionAngle = 90;
        [SerializeField] private  float detectionRadius;
    
    

        private void Start()
        {
            _timer = new CountdownTimer(timerInterval);
            _timer.OnTimerStop += () =>
            {
            
                UpdateTargetPosition(_foundTarget.OrNull());
                _timer.Start();
            };
            _timer.Start();
        }

        void UpdateTargetPosition(GameObject target = null)
        {
            IsTargetPresent = target;
            TargetComponent = target ? target.transform.root.TryGetComponent<T>(out var component) ? component : null : null;
            _foundTarget = target;
            if (!IsTargetPresent || (_lastKnownPosition == TargetPosition && _lastKnownPosition == Vector3.zero)) return;
            _lastKnownPosition = TargetPosition;
            OnTargetChanged.Invoke();
        }

    
    
        private void Update()
        {
        
            if(Physics.Raycast(rayOrigin.position, sensorTarget.transform.position - rayOrigin.transform.position, out RaycastHit hit, Mathf.Infinity, detectionLayer | obstructionLayer))
            {
       
                if( detectionLayer.Contains( hit.collider.gameObject.layer))
                {
                    UpdateTargetPosition(hit.collider.gameObject);
                }
                else
                {
                    UpdateTargetPosition();
                }
           
            }
            else
            {
                UpdateTargetPosition();
            }
        }
        
        
        public bool InFieldOfView (Transform target, float range)
        {
            var directionToTarget = (target.position - transform.position).normalized;
            var angleToPlayer = Vector3.Angle(transform.forward, directionToTarget);
        
            // If the player is outside the detection angle or outside the detection radius, return false
            if(!(angleToPlayer < detectionAngle / 2f) || !(directionToTarget.magnitude < detectionRadius))
                return false;
            
            return true;
        }

    

        private void OnDrawGizmos()
        {
            if (rayOrigin == null) return;
            Gizmos.color = Color.red;
            var go = GameObject.FindGameObjectWithTag("Player");
            Gizmos.DrawRay(rayOrigin.position, go.transform.position - rayOrigin.position);
        
        }
    }
}