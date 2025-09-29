using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPC.GOAP
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

        // ISensor implementation
        public bool CanSeeTarget => _visibleTargets.Count > 0;
        public Transform Target => PrimaryTarget?.transform;

        // Multi-target properties
        public List<T> VisibleTargets => new(_visibleTargets);
        public List<T> AllDetectedTargets => new(_allDetectedTargets);
        public List<T> ObstructedTargets => new(_obstructedTargets);
        public int TargetCount => _visibleTargets.Count;
        public T PrimaryTarget { get; private set; }

        public Vector3 PrimaryTargetPosition => PrimaryTarget != null ? PrimaryTarget.transform.position : Vector3.zero;

        // Events
        public event Action<T> OnTargetDetected = delegate { };
        public event Action<T> OnTargetLost = delegate { };
        public event Action<T> OnTargetBecameVisible = delegate { };
        public event Action<T> OnTargetBecameObstructed = delegate { };
        public event Action<T> OnPrimaryTargetChanged = delegate { };

        private List<T> _visibleTargets = new();
        private readonly List<T> _allDetectedTargets = new();
        private readonly List<T> _obstructedTargets = new();
        private T _previousPrimaryTarget;

        private readonly Dictionary<T, bool> _targetVisibilityStates = new();
        private float _lastUpdateTime;

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

        private void UpdateTargetDetection()
        {
            // Store previous states for comparison
            var previousVisible = new List<T>(_visibleTargets);
            var previousAll = new List<T>(_allDetectedTargets);

            // Clear current lists
            _visibleTargets.Clear();
            _allDetectedTargets.Clear();
            _obstructedTargets.Clear();

            // Find all objects of type T in range
            var colliders = Physics.OverlapSphere(sensorOrigin.position, detectionRadius, detectionLayers);

            foreach (var collider in colliders)
            {
                var targetComponent = collider.GetComponent<T>();
                if (targetComponent != null)
                {
                    _allDetectedTargets.Add(targetComponent);

                    var isVisible = !requireLineOfSight || HasLineOfSight(targetComponent);

                    if (isVisible)
                        _visibleTargets.Add(targetComponent);
                    else
                        _obstructedTargets.Add(targetComponent);

                    // Check for visibility state changes
                    CheckVisibilityStateChange(targetComponent, isVisible);
                }
            }

            // Limit targets if necessary
            if (_visibleTargets.Count > maxTargets)
            {
                if (prioritizeClosest)
                    _visibleTargets = _visibleTargets
                        .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                        .Take(maxTargets)
                        .ToList();
                else
                    _visibleTargets = _visibleTargets.Take(maxTargets).ToList();
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

            if (_visibleTargets.Count == 0)
                PrimaryTarget = null;
            else if (prioritizeClosest)
                PrimaryTarget = _visibleTargets
                    .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                    .FirstOrDefault();
            else
                PrimaryTarget = _visibleTargets.FirstOrDefault();

            // Fire primary target changed event
            if (PrimaryTarget != _previousPrimaryTarget) OnPrimaryTargetChanged.Invoke(PrimaryTarget);
        }

        private void CheckForTargetChanges(List<T> previousVisible, List<T> previousAll)
        {
            // Check for newly detected targets
            var newlyDetected = _allDetectedTargets.Except(previousAll);
            foreach (var target in newlyDetected) OnTargetDetected.Invoke(target);

            // Check for lost targets
            var lostTargets = previousAll.Except(_allDetectedTargets);
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
            if (_visibleTargets.Count == 0) return null;

            return _visibleTargets
                .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                .FirstOrDefault();
        }

        public T GetFarthestTarget()
        {
            if (_visibleTargets.Count == 0) return null;

            return _visibleTargets
                .OrderByDescending(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                .FirstOrDefault();
        }

        public List<T> GetTargetsInRange(float range)
        {
            return _visibleTargets
                .Where(t => Vector3.Distance(sensorOrigin.position, t.transform.position) <= range)
                .ToList();
        }

        public List<T> GetTargetsSortedByDistance()
        {
            return _visibleTargets
                .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                .ToList();
        }

        public bool IsTargetVisible(T target)
        {
            return _visibleTargets.Contains(target);
        }

        public bool IsTargetObstructed(T target)
        {
            return _obstructedTargets.Contains(target);
        }

        public float GetDistanceToTarget(T target)
        {
            if (target == null) return float.MaxValue;
            return Vector3.Distance(sensorOrigin.position, target.transform.position);
        }

        // Configuration methods
        public void SetDetectionRadius(float radius)
        {
            detectionRadius = Mathf.Max(0f, radius);
        }

        public void SetUpdateInterval(float interval)
        {
            updateInterval = Mathf.Max(0.1f, interval);
        }

        public void SetMaxTargets(int max)
        {
            maxTargets = Mathf.Max(1, max);
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            var origin = sensorOrigin != null ? sensorOrigin.position : transform.position;

            // Draw detection radius
            Gizmos.color = detectionRangeColor;
            Gizmos.DrawWireSphere(origin, detectionRadius);

            if (Application.isPlaying)
            {
                // Draw lines to visible targets
                Gizmos.color = visibleTargetColor;
                foreach (var target in _visibleTargets)
                    if (target != null)
                    {
                        Gizmos.DrawLine(origin, target.transform.position);
                        Gizmos.DrawWireSphere(target.transform.position, 0.5f);
                    }

                // Draw lines to obstructed targets
                Gizmos.color = obstructedTargetColor;
                foreach (var target in _obstructedTargets)
                    if (target != null)
                    {
                        Gizmos.DrawLine(origin, target.transform.position);
                        Gizmos.DrawWireCube(target.transform.position, Vector3.one * 0.5f);
                    }

                // Highlight primary target
                if (PrimaryTarget != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(PrimaryTarget.transform.position, 1f);
                }
            }
        }
    }
}