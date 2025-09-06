using System;
using System.Collections;
using Attributes;
using Player.Looking.Player.Looking;
using UnityEngine;

namespace Player.Looking
{
    [RequireComponent(typeof(Rigidbody))]
    public class CameraEffects : MonoBehaviour
    {
        [Header("Configuration")]
        [Required, ScriptableObjectDropdown] 
        public CameraBobConfig cameraBobConfig;
        
        [Required, ScriptableObjectDropdown] 
        public SwayConfig swayConfig;

        [Header("Transform References")]
        [SerializeField, Required] 
        private Transform cameraBobTransform;
        
        [SerializeField, Required] 
        private Transform swayTransform;


        private Vector3 _initialPosition;
        private float _bobTimer;
        private float _swayTimer;
        private Rigidbody _rigidBody;
        private Coroutine _bobCoroutine;

       
        [SerializeField] private  float movementThreshold = 0.1f;
        [SerializeField] private float positionLerpSpeed = 1000f;
        [SerializeField] private  float stopBobLerpSpeed = 10f;
        [SerializeField] private  float swayLerpSpeed = 5f;
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

        public void Sway(Sway sway)
        {
            var swayPosition = CalculateSwayPosition(sway);
            swayTransform.localPosition = Vector3.Lerp(
                swayTransform.localPosition, 
                swayPosition, 
                Time.deltaTime * swayLerpSpeed
            );
        }

        private void Update()
        {
            _swayTimer += Time.deltaTime;
        }

        private void StopCurrentBobCoroutine()
        {
            if (_bobCoroutine != null)
            {
                StopCoroutine(_bobCoroutine);
                _bobCoroutine = null;
            }
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

        private Vector3 CalculateSwayPosition(Sway sway)
        {
            var horizontalSway = Mathf.Cos(_swayTimer * sway.horizontalSwaySpeed) * 
                                sway.horizontalSwayAmount * sway.swayMultiplier;
            
            var verticalSway = Mathf.Sin(_swayTimer * sway.verticalSwaySpeed) * 
                              sway.verticalSwayAmount * sway.swayMultiplier;

            return new Vector3(
                horizontalSway,
                verticalSway,
                swayTransform.localPosition.z
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