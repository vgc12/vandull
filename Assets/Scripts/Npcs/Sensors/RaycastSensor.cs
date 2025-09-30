using System;
using UnityEngine;

namespace Npcs.Sensors
{
    public class RaycastObjectSensor : MonoBehaviour, ISensor
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

        public bool canSeeTarget;
        private bool _wasTargetObstructed;

        private bool _wasTargetVisible;
        public Action OnTargetLost;
        public Action OnTargetObstructed;


        public Action OnTargetSpotted;
        public Action OnTargetUnobstructed;
        public bool IsTargetInRange { get; private set; }
        public bool IsTargetInAngle { get; private set; }
        public bool IsTargetObstructed { get; private set; }
        public float DistanceToTarget { get; private set; }

        public Vector3 LastKnownPosition { get; private set; }

        private void Start()
        {
            // If no sensor origin is specified, use this transform
            if (sensorOrigin == null)
                sensorOrigin = transform;

            // Initialize previous states
            _wasTargetVisible = false;
            _wasTargetObstructed = false;
        }

        private void Update()
        {
            UpdateDetection();
            CheckForStateChanges();
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!showDebugRays || sensorOrigin == null)
                return;

            var origin = sensorOrigin.position;
            var forward = sensorOrigin.forward;

            // Draw detection range circle
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(origin, detectionRange);

            // Draw detection angle cone
            var halfAngle = detectionAngle / 2f;
            var leftBoundary = Quaternion.AngleAxis(-halfAngle, sensorOrigin.up) * forward * detectionRange;
            var rightBoundary = Quaternion.AngleAxis(halfAngle, sensorOrigin.up) * forward * detectionRange;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + leftBoundary);
            Gizmos.DrawLine(origin, origin + rightBoundary);

            // Draw arc for detection cone
            var arcSegments = 20;
            var previousPoint = origin + leftBoundary;
            for (var i = 1; i <= arcSegments; i++)
            {
                var currentAngle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / arcSegments);
                var currentPoint =
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

        public bool CanSeeTarget => canSeeTarget;
        public Transform Target => targetObject;

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

            var directionToTarget = targetObject.position - sensorOrigin.position;
            DistanceToTarget = directionToTarget.magnitude;

            // Check if target is within range
            IsTargetInRange = DistanceToTarget <= detectionRange;
            if (!IsTargetInRange)
                //                VandullLogger.Log("Target not in range! ");
                return;

            // Check if target is within detection angle
            var forwardDirection = sensorOrigin.forward;
            var angleToTarget = Vector3.Angle(forwardDirection, directionToTarget);
            IsTargetInAngle = angleToTarget <= detectionAngle / 2f;

            if (!IsTargetInAngle)
                //                VandullLogger.Log("Target not in angle!" );
                return;

            // Perform raycast to check for obstructions
            RaycastHit hit;
            if (Physics.Raycast(sensorOrigin.position, directionToTarget.normalized, out hit, DistanceToTarget,
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
    }
}