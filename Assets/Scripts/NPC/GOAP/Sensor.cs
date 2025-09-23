using System;
using General;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Sensor : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float timerInterval = 1f;
    private SphereCollider _detectionRange;
    public event Action OnTargetChanged = delegate { };
    
    public Vector3 TargetPosition => _target ? _target.transform.position : Vector3.zero;
    public bool IsTargetInRange => TargetPosition != Vector3.zero;

    private GameObject _target;
 
    private Vector3 _lastKnownPosition;

    private CountdownTimer _timer;

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

    void UpdateTargetPosition(GameObject target = null)
    {
        _target = target;
        if (!IsTargetInRange || (_lastKnownPosition == TargetPosition && _lastKnownPosition == Vector3.zero)) return;
        _lastKnownPosition = TargetPosition;
        OnTargetChanged.Invoke();
    }


    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return;
        UpdateTargetPosition(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player")) return;
        UpdateTargetPosition();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsTargetInRange ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}