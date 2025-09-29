using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NPC.GOAP
{
    public class CoverPointSensor : MultiTargetTypeSensor<CoverPoint>
    {

    }

    public class MultiTargetTypeSensor<T> : MonoBehaviour, ISensor where T : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private LayerMask detectionLayers = -1;
        [SerializeField] private float updateInterval = 0.5f;
    
        [Header("Line of Sight")]
        [SerializeField] private bool requireLineOfSight = true;
        [SerializeField] private LayerMask obstructionLayers = 1;
        [SerializeField] private Transform sensorOrigin;
    
        [Header("Target Selection")]
        [SerializeField] private bool prioritizeClosest = true;
        [SerializeField] private int maxTargets = 10;
    
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color detectionRangeColor = Color.cyan;
        [SerializeField] private Color visibleTargetColor = Color.green;
        [SerializeField] private Color obstructedTargetColor = Color.red;
    
        // ISensor implementation
        public bool CanSeeTarget => visibleTargets.Count > 0;
        public Transform Target => primaryTarget?.transform;
    
        // Multi-target properties
        public List<T> VisibleTargets => new List<T>(visibleTargets);
        public List<T> AllDetectedTargets => new List<T>(allDetectedTargets);
        public List<T> ObstructedTargets => new List<T>(obstructedTargets);
        public int TargetCount => visibleTargets.Count;
        public T PrimaryTarget => primaryTarget;
        public Vector3 PrimaryTargetPosition => primaryTarget != null ? primaryTarget.transform.position : Vector3.zero;
    
        // Events
        public event Action<T> OnTargetDetected = delegate { };
        public event Action<T> OnTargetLost = delegate { };
        public event Action<T> OnTargetBecameVisible = delegate { };
        public event Action<T> OnTargetBecameObstructed = delegate { };
        public event Action<T> OnPrimaryTargetChanged = delegate { };
    
        private List<T> visibleTargets = new List<T>();
        private List<T> allDetectedTargets = new List<T>();
        private List<T> obstructedTargets = new List<T>();
        private T primaryTarget;
        private T previousPrimaryTarget;
    
        private Dictionary<T, bool> targetVisibilityStates = new Dictionary<T, bool>();
        private float lastUpdateTime;
    
        private void Awake()
        {
            if (sensorOrigin == null)
                sensorOrigin = transform;
        }
    
        private void Start()
        {
            lastUpdateTime = Time.time;
            UpdateTargetDetection();
        }
    
        private void Update()
        {
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateTargetDetection();
                lastUpdateTime = Time.time;
            }
        }
    
        private void UpdateTargetDetection()
        {
            // Store previous states for comparison
            var previousVisible = new List<T>(visibleTargets);
            var previousAll = new List<T>(allDetectedTargets);
        
            // Clear current lists
            visibleTargets.Clear();
            allDetectedTargets.Clear();
            obstructedTargets.Clear();
        
            // Find all objects of type T in range
            var colliders = Physics.OverlapSphere(sensorOrigin.position, detectionRadius, detectionLayers);
        
            foreach (var collider in colliders)
            {
                var targetComponent = collider.GetComponent<T>();
                if (targetComponent != null)
                {
                    allDetectedTargets.Add(targetComponent);
                
                    bool isVisible = !requireLineOfSight || HasLineOfSight(targetComponent);
                
                    if (isVisible)
                    {
                        visibleTargets.Add(targetComponent);
                    }
                    else
                    {
                        obstructedTargets.Add(targetComponent);
                    }
                
                    // Check for visibility state changes
                    CheckVisibilityStateChange(targetComponent, isVisible);
                }
            }
        
            // Limit targets if necessary
            if (visibleTargets.Count > maxTargets)
            {
                if (prioritizeClosest)
                {
                    visibleTargets = visibleTargets
                        .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                        .Take(maxTargets)
                        .ToList();
                }
                else
                {
                    visibleTargets = visibleTargets.Take(maxTargets).ToList();
                }
            }
        
            // Update primary target
            UpdatePrimaryTarget();
        
            // Fire detection/loss events
            CheckForTargetChanges(previousVisible, previousAll);
        }
    
        private bool HasLineOfSight(T target)
        {
            Vector3 directionToTarget = target.transform.position - sensorOrigin.position;
            float distanceToTarget = directionToTarget.magnitude;
        
            // Raycast to check for obstructions
            RaycastHit hit;
            if (Physics.Raycast(sensorOrigin.position, directionToTarget.normalized, out hit, distanceToTarget, obstructionLayers))
            {
                // Check if we hit the target itself (no obstruction)
                return hit.collider.gameObject == target.gameObject;
            }
        
            return true; // No obstruction found
        }
    
        private void CheckVisibilityStateChange(T target, bool isCurrentlyVisible)
        {
            bool wasVisible = targetVisibilityStates.ContainsKey(target) && targetVisibilityStates[target];
            targetVisibilityStates[target] = isCurrentlyVisible;
        
            if (wasVisible != isCurrentlyVisible)
            {
                if (isCurrentlyVisible)
                {
                    OnTargetBecameVisible.Invoke(target);
                }
                else
                {
                    OnTargetBecameObstructed.Invoke(target);
                }
            }
        }
    
        private void UpdatePrimaryTarget()
        {
            previousPrimaryTarget = primaryTarget;
        
            if (visibleTargets.Count == 0)
            {
                primaryTarget = null;
            }
            else if (prioritizeClosest)
            {
                primaryTarget = visibleTargets
                    .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                    .FirstOrDefault();
            }
            else
            {
                primaryTarget = visibleTargets.FirstOrDefault();
            }
        
            // Fire primary target changed event
            if (primaryTarget != previousPrimaryTarget)
            {
                OnPrimaryTargetChanged.Invoke(primaryTarget);
            }
        }
    
        private void CheckForTargetChanges(List<T> previousVisible, List<T> previousAll)
        {
            // Check for newly detected targets
            var newlyDetected = allDetectedTargets.Except(previousAll);
            foreach (var target in newlyDetected)
            {
                OnTargetDetected.Invoke(target);
            }
        
            // Check for lost targets
            var lostTargets = previousAll.Except(allDetectedTargets);
            foreach (var target in lostTargets)
            {
                OnTargetLost.Invoke(target);
                // Remove from visibility states to prevent memory leaks
                if (targetVisibilityStates.ContainsKey(target))
                {
                    targetVisibilityStates.Remove(target);
                }
            }
        }
    
        // Public utility methods
        public T GetClosestTarget()
        {
            if (visibleTargets.Count == 0) return null;
        
            return visibleTargets
                .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                .FirstOrDefault();
        }
    
        public T GetFarthestTarget()
        {
            if (visibleTargets.Count == 0) return null;
        
            return visibleTargets
                .OrderByDescending(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                .FirstOrDefault();
        }
    
        public List<T> GetTargetsInRange(float range)
        {
            return visibleTargets
                .Where(t => Vector3.Distance(sensorOrigin.position, t.transform.position) <= range)
                .ToList();
        }
    
        public List<T> GetTargetsSortedByDistance()
        {
            return visibleTargets
                .OrderBy(t => Vector3.Distance(sensorOrigin.position, t.transform.position))
                .ToList();
        }
    
        public bool IsTargetVisible(T target)
        {
            return visibleTargets.Contains(target);
        }
    
        public bool IsTargetObstructed(T target)
        {
            return obstructedTargets.Contains(target);
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
        
            Vector3 origin = sensorOrigin != null ? sensorOrigin.position : transform.position;
        
            // Draw detection radius
            Gizmos.color = detectionRangeColor;
            Gizmos.DrawWireSphere(origin, detectionRadius);
        
            if (Application.isPlaying)
            {
                // Draw lines to visible targets
                Gizmos.color = visibleTargetColor;
                foreach (var target in visibleTargets)
                {
                    if (target != null)
                    {
                        Gizmos.DrawLine(origin, target.transform.position);
                        Gizmos.DrawWireSphere(target.transform.position, 0.5f);
                    }
                }
            
                // Draw lines to obstructed targets
                Gizmos.color = obstructedTargetColor;
                foreach (var target in obstructedTargets)
                {
                    if (target != null)
                    {
                        Gizmos.DrawLine(origin, target.transform.position);
                        Gizmos.DrawWireCube(target.transform.position, Vector3.one * 0.5f);
                    }
                }
            
                // Highlight primary target
                if (primaryTarget != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(primaryTarget.transform.position, 1f);
                }
            }
        }
    }
}