using System;
using System.Collections.Generic;
using System.Linq;
using General.Extensions;
using UnityEngine;

namespace Npcs.Sensors
{
    public class MultiTargetTypeSensor<T> : MonoBehaviour, ISensor where T : MonoBehaviour
    {
        [Header("Detection Settings")] [SerializeField]
        private float detectionRadius = 10f;

        [SerializeField] private LayerMask detectionLayers = -1;
        [SerializeField] private float updateInterval = 0.5f;

        [Header("Line of Sight")] [SerializeField]
        private bool requireLineOfSight = true;

        [SerializeField] private LayerMask obstructionLayers = 1;
        [SerializeField] private Transform sensorOrigin;

        [Header("Target Selection")] [SerializeField]
        private bool prioritizeClosest = true;

        [SerializeField] private int maxTargets = 10;

        [Header("Debug")] [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color detectionRangeColor = Color.cyan;
        [SerializeField] private Color visibleTargetColor = Color.green;
        [SerializeField] private Color obstructedTargetColor = Color.red;

        private readonly Dictionary<T, bool> _targetVisibilityStates = new();
        private float _lastUpdateTime;
        private T _previousPrimaryTarget;

        // Multi-target properties
        public List<T> VisibleTargets { get; private set; } = new();

        public List<T> AllDetectedTargets { get; } = new();

        public List<T> ObstructedTargets { get; } = new();

        public int TargetCount => VisibleTargets.Count;
        public T PrimaryTarget { get; private set; }

        public Vector3 PrimaryTargetPosition => PrimaryTarget != null ? PrimaryTarget.transform.position : Vector3.zero;

        private void Awake()
        {
            if (sensorOrigin == null)
                sensorOrigin = transform;
        }


        private void Start()
        {
            _lastUpdateTime = Time.time;
            UpdateTargetDetection();
        }

        private void Update()
        {
            if (Time.time - _lastUpdateTime >= updateInterval)
            {
                UpdateTargetDetection();
                _lastUpdateTime = Time.time;
            }
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            var origin = sensorOrigin != null ? sensorOrigin.position : transform.position;

            // Draw detection radius
            Gizmos.color = detectionRangeColor;
            Gizmos.DrawWireSphere(origin, detectionRadius);

            if (!Application.isPlaying) return;
            // Draw lines to visible targets
            Gizmos.color = visibleTargetColor;
            foreach (var target in VisibleTargets)
            {
                if (target != null)
                {
                    Gizmos.DrawLine(origin, target.transform.position);
                    Gizmos.DrawWireSphere(target.transform.position, 0.5f);
                }
            }

            // Draw lines to obstructed targets
            Gizmos.color = obstructedTargetColor;
            foreach (var target in ObstructedTargets)
            {
                if (target != null)
                {
                    Gizmos.DrawLine(origin, target.transform.position);
                    Gizmos.DrawWireCube(target.transform.position, Vector3.one * 0.5f);
                }
            }

            // Highlight primary target
            if (PrimaryTarget != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(PrimaryTarget.transform.position, 1f);
            }
        }

        // ISensor implementation
        public virtual bool CanSeeTarget => VisibleTargets.Count > 0;
        public Transform Target => PrimaryTarget?.transform;

        public void OnDisable()
        {
            VisibleTargets.Clear();
            AllDetectedTargets.Clear();
            ObstructedTargets.Clear();
            PrimaryTarget = null;
            _targetVisibilityStates.Clear();
        }

        // Events
        public event Action<T> OnTargetDetected = delegate { };
        public event Action<T> OnTargetLost = delegate { };
        public event Action<T> OnTargetBecameVisible = delegate { };
        public event Action<T> OnTargetBecameObstructed = delegate { };
        public event Action<T> OnPrimaryTargetChanged = delegate { };

        private void UpdateTargetDetection()
        {
            // Store previous states for comparison
            var previousVisible = new List<T>(VisibleTargets);
            var previousAll = new List<T>(AllDetectedTargets);

            // Clear current lists
            VisibleTargets.Clear();
            AllDetectedTargets.Clear();
            ObstructedTargets.Clear();

            // Find all objects of type T in range
            var colliders = Physics.OverlapSphere(sensorOrigin.position, detectionRadius, detectionLayers);
            
            
            foreach (var col in colliders)
            {
           
                var targetComponent = col.GetComponentInParent<T>();
                
                if (!targetComponent) continue;
                AllDetectedTargets.Add(targetComponent);

                var isVisible = !requireLineOfSight || HasLineOfSight(targetComponent);

                if (isVisible)
                    VisibleTargets.Add(targetComponent);
                else
                    ObstructedTargets.Add(targetComponent);

                // Check for visibility state changes
                CheckVisibilityStateChange(targetComponent, isVisible);
            }

            // Limit targets if necessary
            if (VisibleTargets.Count > maxTargets)
            {
                if (prioritizeClosest)
                    VisibleTargets = VisibleTargets
                        .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                        .Take(maxTargets)
                        .ToList();
                else
                    VisibleTargets = VisibleTargets.Take(maxTargets).ToList();
            }

            // Update primary target
            UpdatePrimaryTarget();

            // Fire detection/loss events
            CheckForTargetChanges(previousVisible, previousAll);
        }

        private bool HasLineOfSight(T target)
        {
            var directionToTarget = target.transform.position - sensorOrigin.position;
            var distanceToTarget = directionToTarget.magnitude;

            // Raycast to check for obstructions
            RaycastHit hit;
            if (Physics.Raycast(sensorOrigin.position, directionToTarget.normalized, out hit, distanceToTarget,
                    obstructionLayers))
                // Check if we hit the target itself (no obstruction)
                return hit.collider.gameObject == target.gameObject;

            return true; // No obstruction found
        }

        private void CheckVisibilityStateChange(T target, bool isCurrentlyVisible)
        {
            var wasVisible = _targetVisibilityStates.ContainsKey(target) && _targetVisibilityStates[target];
            _targetVisibilityStates[target] = isCurrentlyVisible;

            if (wasVisible != isCurrentlyVisible)
            {
                if (isCurrentlyVisible)
                    OnTargetBecameVisible.Invoke(target);
                else
                    OnTargetBecameObstructed.Invoke(target);
            }
        }

        private void UpdatePrimaryTarget()
        {
            _previousPrimaryTarget = PrimaryTarget;

            if (VisibleTargets.Count == 0)
                PrimaryTarget = null;
            else if (prioritizeClosest)
                PrimaryTarget = VisibleTargets
                    .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                    .FirstOrDefault();
            else
                PrimaryTarget = VisibleTargets.FirstOrDefault();

            // Fire primary target changed event
            if (PrimaryTarget != _previousPrimaryTarget) OnPrimaryTargetChanged.Invoke(PrimaryTarget);
        }

        private void CheckForTargetChanges(List<T> previousVisible, List<T> previousAll)
        {
            // Check for newly detected targets
            var newlyDetected = AllDetectedTargets.Except(previousAll);
            foreach (var target in newlyDetected) OnTargetDetected.Invoke(target);

            // Check for lost targets
            var lostTargets = previousAll.Except(AllDetectedTargets);
            foreach (var target in lostTargets)
            {
                OnTargetLost.Invoke(target);
                // Remove from visibility states to prevent memory leaks
                if (_targetVisibilityStates.ContainsKey(target)) _targetVisibilityStates.Remove(target);
            }
        }

        // Public utility methods
        public T GetClosestTarget()
        {
            if (VisibleTargets.Count == 0) return null;

            return VisibleTargets
                .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                .FirstOrDefault();
        }

        public T GetFarthestTarget()
        {
            if (VisibleTargets.Count == 0) return null;

            return VisibleTargets
                .OrderByDescending(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                .FirstOrDefault();
        }

        public List<T> GetTargetsInRange(float range) =>
            VisibleTargets
                .Where(t => Vector3.Distance(sensorOrigin.position, t.transform.position) <= range)
                .ToList();

        public List<T> GetTargetsSortedByDistance() =>
            VisibleTargets
                .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                .ToList();

        public bool IsTargetVisible(T target) => VisibleTargets.Contains(target);

        public bool IsTargetObstructed(T target) => ObstructedTargets.Contains(target);


        public float GetDistanceToTarget(T target)
        {
            if (target == null) return float.MaxValue;
            return Vector3.Distance(sensorOrigin.position, target.transform.position);
        }

        // Configuration methods
        public void SetDetectionRadius(float radius) => detectionRadius = Mathf.Max(0f, radius);

        public void SetUpdateInterval(float interval) => updateInterval = Mathf.Max(0.1f, interval);

        public void SetMaxTargets(int max) => maxTargets = Mathf.Max(1, max);
    }
}