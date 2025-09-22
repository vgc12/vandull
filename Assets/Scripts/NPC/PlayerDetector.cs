using System;
using General;
using NPC.Strategies;
using UnityEngine;

namespace NPC
{
    public class PlayerDetector : MonoBehaviour
    {
        [SerializeField] private float detectionAngle = 60f;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float innerDetectionRange = 5f;
        [SerializeField] private float detectionCooldown = 1f;
        
        public Transform Player { get; private set; }
        private CountdownTimer _detectionTimer;
        
        private IDetectionStrategy _detectionStrategy;

        private void Awake()
        {
            _detectionTimer = new CountdownTimer(detectionCooldown);
            Player = GameObject.FindGameObjectWithTag("Player").transform;
            _detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRange, innerDetectionRange);
            
        }
        
        private void Update()
        {
            _detectionTimer.Tick(Time.deltaTime);

        }
        
        public bool CanDetectPlayer() => _detectionTimer.IsRunning || _detectionStrategy.Execute(Player, transform, _detectionTimer);
        
        public void SetDetectionStrategy(IDetectionStrategy strategy) => _detectionStrategy = strategy;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.DrawWireSphere(transform.position, innerDetectionRange);
            
            var forwardConeDirection = Quaternion.Euler(0, detectionAngle / 2, 0) * transform.forward * detectionRange;
            var backwardConeDirection = Quaternion.Euler(0, -detectionAngle / 2, 0) * transform.forward * detectionRange;
            
            // Draw lines to represent the cone
            Gizmos.DrawLine(transform.position, transform.position + forwardConeDirection);
            Gizmos.DrawLine(transform.position, transform.position + backwardConeDirection);
        }
    }
}