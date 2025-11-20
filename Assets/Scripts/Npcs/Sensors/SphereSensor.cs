using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Npcs.Sensors
{
    public class MultiTargetSensor<T> : MonoBehaviour, ISensor where T : MonoBehaviour
    {
        [Header("Patrol Settings")] [SerializeField]
        private List<T> patrolPoints = new();

        [SerializeField] private bool loopPatrol = true;
        [SerializeField] private bool randomizeStartPoint;

        [Header("Debug")] [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color patrolPointColor = Color.blue;
        [SerializeField] private Color currentPointColor = Color.green;
        [SerializeField] private Color lineColor = Color.cyan;

        public T CurrentPatrolPoint { get; private set; }

        public int CurrentPointIndex { get; private set; } = -1;

        public int TotalPoints => patrolPoints.Count;

        private void Awake()
        {
            InitializePatrolPoints();
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || patrolPoints.Count == 0) return;

            // Draw patrol points
            for (var i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] == null) continue;

                var color = Application.isPlaying && i == CurrentPointIndex ? currentPointColor : patrolPointColor;
                Gizmos.color = color;
                Gizmos.DrawWireSphere(patrolPoints[i].transform.position, 0.5f);
            }

            // Draw lines between patrol points
            if (patrolPoints.Count <= 1) return;
            {
                Gizmos.color = lineColor;
                for (var i = 0; i < patrolPoints.Count; i++)
                {
                    if (patrolPoints[i] == null) continue;

                    var nextIndex = (i + 1) % patrolPoints.Count;
                    if (patrolPoints[nextIndex] == null) continue;

                    Gizmos.DrawLine(patrolPoints[i].transform.position, patrolPoints[nextIndex].transform.position);
                }
            }
        }

        public bool CanSeeTarget => CurrentPatrolPoint != null;
        public Transform Target => CurrentPatrolPoint?.transform;

        public event Action<T> OnPatrolPointChanged = delegate { };
        public event Action OnPatrolLooped = delegate { };

        private void InitializePatrolPoints()
        {
            if (patrolPoints.Count == 0) patrolPoints.AddRange(GetComponentsInChildren<T>());

            foreach (var p in patrolPoints)
                p.transform.SetParent(null, true);

            CurrentPointIndex = randomizeStartPoint ? Random.Range(0, patrolPoints.Count) : 0;

            CurrentPatrolPoint = patrolPoints[CurrentPointIndex].GetComponent<T>();
        }

        public T GetNextPatrolPoint()
        {
            if (patrolPoints.Count == 0) return null;

            var nextIndex = CurrentPointIndex + 1;

            // Check if we've looped
            if (nextIndex >= patrolPoints.Count)
            {
                if (!loopPatrol)
                    return null;

                nextIndex = 0;
                OnPatrolLooped.Invoke();
            }

            CurrentPointIndex = nextIndex;
            CurrentPatrolPoint = patrolPoints[CurrentPointIndex].GetComponent<T>();
            OnPatrolPointChanged.Invoke(CurrentPatrolPoint);

            return CurrentPatrolPoint;
        }

        public T GetPreviousPatrolPoint()
        {
            if (patrolPoints.Count == 0) return null;

            var prevIndex = CurrentPointIndex - 1;

            // Check if we've looped backwards
            if (prevIndex < 0)
            {
                if (!loopPatrol)
                    return null;

                prevIndex = patrolPoints.Count - 1;
                OnPatrolLooped.Invoke();
            }

            CurrentPointIndex = prevIndex;
            CurrentPatrolPoint = patrolPoints[CurrentPointIndex].GetComponent<T>();
            OnPatrolPointChanged.Invoke(CurrentPatrolPoint);

            return CurrentPatrolPoint;
        }

        public T GetPatrolPointAtIndex(int index)
        {
            if (index < 0 || index >= patrolPoints.Count) return null;

            CurrentPointIndex = index;
            CurrentPatrolPoint = patrolPoints[CurrentPointIndex].GetComponent<T>();
            OnPatrolPointChanged.Invoke(CurrentPatrolPoint);

            return CurrentPatrolPoint;
        }

        public void AddPatrolPoint(T point)
        {
            if (point != null && point.GetComponent<T>() != null)
            {
                patrolPoints.Add(point);
                if (CurrentPatrolPoint == null)
                    InitializePatrolPoints();
            }
        }

        public void InsertPatrolPoint(T point, int index)
        {
            if (point != null && point.GetComponent<T>() != null && index >= 0 && index <= patrolPoints.Count)
            {
                patrolPoints.Insert(index, point);
                if (CurrentPatrolPoint == null)
                    InitializePatrolPoints();
            }
        }

        public void RemovePatrolPoint(T point)
        {
            if (!patrolPoints.Remove(point)) return;
            if (CurrentPatrolPoint == null || CurrentPatrolPoint != point) return;
            CurrentPointIndex = Mathf.Max(0, CurrentPointIndex - 1);
            CurrentPatrolPoint = patrolPoints.Count > 0 ? patrolPoints[CurrentPointIndex].GetComponent<T>() : null;
        }

        public void RemovePatrolPointAtIndex(int index)
        {
            if (index < 0 || index >= patrolPoints.Count) return;
            patrolPoints.RemoveAt(index);
            if (CurrentPointIndex >= patrolPoints.Count)
                CurrentPointIndex = Mathf.Max(0, patrolPoints.Count - 1);

            CurrentPatrolPoint = patrolPoints.Count > 0 ? patrolPoints[CurrentPointIndex].GetComponent<T>() : null;
        }

        public void ClearPatrolPoints()
        {
            patrolPoints.Clear();
            CurrentPointIndex = -1;
            CurrentPatrolPoint = null;
        }

        public List<T> GetAllPatrolPoints()
        {
            var result = new List<T>();
            foreach (var point in patrolPoints)
                if (point != null)
                {
                    var component = point.GetComponent<T>();
                    if (component != null)
                        result.Add(component);
                }

            return result;
        }

        public T GetClosestPatrolPoint(Vector3 position)
        {
            if (patrolPoints.Count == 0) return null;

            T closest = null;
            var minDistance = float.MaxValue;

            foreach (var point in patrolPoints)
            {
                if (point == null) continue;

                var component = point.GetComponent<T>();
                if (component == null) continue;

                var distance = Vector3.Distance(position, point.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = component;
                }
            }

            return closest;
        }

        public float GetDistanceToCurrentPoint(Vector3 position)
        {
            return CurrentPatrolPoint == null
                ? float.MaxValue
                : Vector3.Distance(position, CurrentPatrolPoint.transform.position);
        }


        public void SpawnTarget()
        {
            var newGameObject = new GameObject($"Target ({typeof(T).Name})");
            newGameObject.transform.position = transform.position + Vector3.forward * 2f;

            newGameObject.AddComponent<T>();
            newGameObject.transform.SetParent(transform);
            var sphereCollider = newGameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(newGameObject, $"Spawn {typeof(T).Name}");
#endif
        }
    }
}