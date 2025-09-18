using System;
using System.Collections;
using Attributes;
using UnityEngine;

namespace Player.Looking
{
    [RequireComponent(typeof(Rigidbody))]
    public class CameraBobber : MonoBehaviour
    {
        [Header("Configuration")]
        [Required] 
        public CameraBobConfig cameraBobConfig;

        [Header("Transform References")]
        [SerializeField, Required] 
        private Transform cameraBobTransform;

        private Vector3 _initialPosition;
        private float _bobTimer;
        private Rigidbody _rigidBody;
        private Coroutine _bobCoroutine;

       
        [SerializeField] private  float movementThreshold = 0.1f;
        [SerializeField] private float positionLerpSpeed = 1000f;
        [SerializeField] private  float stopBobLerpSpeed = 10f;
       
        [SerializeField] private float positionSnapThreshold = 0.01f;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _initialPosition = cameraBobTransform.localPosition;
        }

        public void CameraBob(CameraBobSetting cameraBobSetting)
        {
            StopCurrentBobCoroutine();
            
            var velocity = _rigidBody.linearVelocity;
            var horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            var isMoving = horizontalSpeed > movementThreshold;

            if (!isMoving) return;

            UpdateBobTimer(cameraBobSetting, horizontalSpeed);
            ApplyBobMovement(cameraBobSetting, horizontalSpeed);
        }

        public void StopBobbing()
        {
            if (_bobCoroutine != null) return;
            
            _bobCoroutine = StartCoroutine(LerpToPosition(_initialPosition));
            _bobTimer = 0f;
        }
        

        private void StopCurrentBobCoroutine()
        {
            if (_bobCoroutine == null) return;
            StopCoroutine(_bobCoroutine);
            _bobCoroutine = null;
        }

        private void UpdateBobTimer(CameraBobSetting cameraBobSetting, float horizontalSpeed)
        {
            _bobTimer += Time.deltaTime * cameraBobSetting.frequency * 
                       Mathf.Min(horizontalSpeed, cameraBobSetting.maxSpeed);
        }

        private void ApplyBobMovement(CameraBobSetting cameraBobSetting, float horizontalSpeed)
        {
            var horizontalBob = Mathf.Sin(_bobTimer) * cameraBobSetting.horizontalAmplitude;
            var verticalBob = Mathf.Sin(_bobTimer * 2) * cameraBobSetting.verticalAmplitude;
            
            var speedMultiplier = Mathf.Min(horizontalSpeed / cameraBobSetting.speedCurve, 1f);
            
            var bobOffset = new Vector3(
                horizontalBob * speedMultiplier,
                verticalBob * speedMultiplier,
                0
            );

            var targetPosition = _initialPosition + bobOffset;
            cameraBobTransform.localPosition = Vector3.Lerp(
                cameraBobTransform.localPosition, 
                targetPosition,
                Time.deltaTime * positionLerpSpeed
            );
        }
        

        private IEnumerator LerpToPosition(Vector3 targetPosition)
        {
            while (Vector3.Distance(cameraBobTransform.localPosition, targetPosition) > positionSnapThreshold)
            {
                cameraBobTransform.localPosition = Vector3.Lerp(
                    cameraBobTransform.localPosition, 
                    targetPosition, 
                    Time.deltaTime * stopBobLerpSpeed
                );

                yield return null;
            }

            cameraBobTransform.localPosition = targetPosition;
            _bobCoroutine = null;
        }
    }
}