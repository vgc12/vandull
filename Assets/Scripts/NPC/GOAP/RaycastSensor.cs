using System;
using Attributes;
using General;
using Player;
using Unity.VisualScripting;
using UnityEngine;

namespace NPC.GOAP
{


    public class EnemyObjectSensor : MonoBehaviour, ISensor
    {
        [Header("Detection Settings")] [SerializeField]
        private Transform targetObject;

        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float detectionAngle = 60f;
        [SerializeField] private LayerMask obstructionLayers = 1;

        [Header("Sensor Origin")] [SerializeField]
        private Transform sensorOrigin;

        [Header("Debug")] [SerializeField] private bool showDebugRays = true;
        [SerializeField] private Color visibleColor = Color.green;
        [SerializeField] private Color obstructedColor = Color.red;
        [SerializeField] private Color outOfRangeColor = Color.yellow;

        public bool CanSeeTarget => canSeeTarget;

        public bool canSeeTarget;
        public bool IsTargetInRange { get; private set; }
        public bool IsTargetInAngle { get; private set; }
        public bool IsTargetObstructed { get; private set; }
        public float DistanceToTarget { get; private set; }
        public Transform Target => targetObject;
        
        public Vector3 LastKnownPosition { get; private set; }


        public System.Action OnTargetSpotted;
        public System.Action OnTargetLost;
        public System.Action OnTargetObstructed;
        public System.Action OnTargetUnobstructed;

        private bool _wasTargetVisible;
        private bool _wasTargetObstructed;

        void Start()
        {
            // If no sensor origin is specified, use this transform
            if (sensorOrigin == null)
                sensorOrigin = transform;

            // Initialize previous states
            _wasTargetVisible = false;
            _wasTargetObstructed = false;
        }

        void Update()
        {
            UpdateDetection();
            CheckForStateChanges();
        }

        private void UpdateDetection()
        {
            // Reset detection states
            canSeeTarget = false;
            IsTargetInRange = false;
            IsTargetInAngle = false;
            IsTargetObstructed = false;
            DistanceToTarget = 0f;

            // Early exit if no target is assigned
            if (targetObject == null)
                return;

            Vector3 directionToTarget = targetObject.position - sensorOrigin.position;
            DistanceToTarget = directionToTarget.magnitude;

            // Check if target is within range
            IsTargetInRange = DistanceToTarget <= detectionRange;
            if (!IsTargetInRange)
            {
//                VandullLogger.Log("Target not in range! ");
                return;
            }

            // Check if target is within detection angle
            Vector3 forwardDirection = sensorOrigin.forward;
            float angleToTarget = Vector3.Angle(forwardDirection, directionToTarget);
            IsTargetInAngle = angleToTarget <= detectionAngle / 2f;

            if (!IsTargetInAngle)
            {
//                VandullLogger.Log("Target not in angle!" );
                return;
            }

            // Perform raycast to check for obstructions
RaycastHit hit;
            if (Physics.Raycast(sensorOrigin.position, directionToTarget.normalized,out hit, DistanceToTarget,
                    obstructionLayers))
            {
               
             //   VandullLogger.LogWarning(hit.collider.gameObject.name);
                IsTargetObstructed = true;
            }
            else
            {
                LastKnownPosition = targetObject.position;
                canSeeTarget = true;
            }
        }

        private void CheckForStateChanges()
        {
            // Check if target visibility changed
            if (canSeeTarget != _wasTargetVisible)
            {
                if (canSeeTarget)
                    OnTargetSpotted?.Invoke();
                else
                    OnTargetLost?.Invoke();

                _wasTargetVisible = canSeeTarget;
            }

            // Check if obstruction state changed
            if (IsTargetObstructed != _wasTargetObstructed)
            {
                if (IsTargetObstructed)
                    OnTargetObstructed?.Invoke();
                else
                    OnTargetUnobstructed?.Invoke();

                _wasTargetObstructed = IsTargetObstructed;
            }
        }

        // Public methods for external control
        public void SetTarget(Transform newTarget)
        {
            targetObject = newTarget;
        }

        public void SetDetectionRange(float newRange)
        {
            detectionRange = Mathf.Max(0f, newRange);
        }

        public void SetDetectionAngle(float newAngle)
        {
            detectionAngle = Mathf.Clamp(newAngle, 0f, 360f);
        }

        public void SetObstructionLayers(LayerMask newLayers)
        {
            obstructionLayers = newLayers;
        }

        // Debug visualization
        void OnDrawGizmos()
        {
            if (!showDebugRays || sensorOrigin == null)
                return;

            Vector3 origin = sensorOrigin.position;
            Vector3 forward = sensorOrigin.forward;

            // Draw detection range circle
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(origin, detectionRange);

            // Draw detection angle cone
            float halfAngle = detectionAngle / 2f;
            Vector3 leftBoundary = Quaternion.AngleAxis(-halfAngle, sensorOrigin.up) * forward * detectionRange;
            Vector3 rightBoundary = Quaternion.AngleAxis(halfAngle, sensorOrigin.up) * forward * detectionRange;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + leftBoundary);
            Gizmos.DrawLine(origin, origin + rightBoundary);

            // Draw arc for detection cone
            int arcSegments = 20;
            Vector3 previousPoint = origin + leftBoundary;
            for (int i = 1; i <= arcSegments; i++)
            {
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / arcSegments);
                Vector3 currentPoint =
                    origin + Quaternion.AngleAxis(currentAngle, sensorOrigin.up) * forward * detectionRange;
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }

            // Draw line to target if it exists
            if (targetObject != null)
            {
                if (canSeeTarget)
                    Gizmos.color = visibleColor;
                else if (IsTargetObstructed)
                    Gizmos.color = obstructedColor;
                else
                    Gizmos.color = outOfRangeColor;

                Gizmos.DrawLine(origin, targetObject.position);

                // Draw a small sphere at target position
                Gizmos.DrawWireSphere(targetObject.position, 0.5f);
            }
        }
    }
}

    
    