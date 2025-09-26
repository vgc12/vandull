using System;
using System.Collections.Generic;
using General;
using UnityEngine;


public class CoverPointSensor  : SphereSensor<CoverPoint>
{ 
}

[RequireComponent(typeof(SphereCollider))]
public abstract class SphereSensor <T> :  MonoBehaviour, ISensor<T> where T : Component 
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float timerInterval = 1f;
    [SerializeField] private LayerMask detectionLayer;
    private SphereCollider _detectionRange;
    public event Action OnTargetChanged = delegate { };

    public T TargetComponent { get; private set; }
    public Vector3 TargetPosition => _target ? _target.transform.position : Vector3.zero;
    public List<Vector3> TargetPositions { get; private set; } = new();

    public bool IsTargetPresent => TargetPosition != Vector3.zero;

    private GameObject _target;

    private Vector3 _lastKnownPosition;

    private CountdownTimer _timer;


    public void GetAllInRange(float range)
    {
        var points = new List<Vector3>();
        GameObject closest = null;
        var results = Physics.OverlapSphere(transform.position, range, detectionLayer);


        var minDistance = float.MaxValue;
        foreach (var col in results)
        {
            points.Add(col.transform.position);
            var distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = col.gameObject;
            }
        }

        TargetPositions = points;
        _target = closest;
        TargetComponent = _target ? _target.TryGetComponent<T>(out var component) ? component : null : null;
    }


    private void Awake()
    {
        _detectionRange = GetComponent<SphereCollider>();
        _detectionRange.isTrigger = true;
        _detectionRange.radius = detectionRadius;
    }

    private void Start()
    {
        _timer = new CountdownTimer(timerInterval);
        _timer.OnTimerStop += () =>
        {
            UpdateTargetPosition(_target.OrNull());
            _timer.Start();
        };
        _timer.Start();
    }

    private void Update()
    {
        _timer.Tick(Time.deltaTime);
    }

    private void UpdateTargetPosition(GameObject target = null)
    {
        GetAllInRange(detectionRadius);
        VandullLogger.Log($"Sensor found target: {_target}");
        if (!IsTargetPresent || (_lastKnownPosition == TargetPosition && _lastKnownPosition == Vector3.zero)) return;
        _lastKnownPosition = TargetPosition;
        OnTargetChanged.Invoke();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (detectionLayer.Contains(other.gameObject.layer))
            UpdateTargetPosition(other.gameObject);
    }


    private void OnTriggerExit(Collider other)
    {
        if (detectionLayer.Contains(other.gameObject.layer))
            UpdateTargetPosition();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsTargetPresent ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

public interface ISensor<T> where T : Component
{
    T TargetComponent { get; }
    Vector3 TargetPosition { get; }
    bool IsTargetPresent { get; }
    event Action OnTargetChanged;
}
