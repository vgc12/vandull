using Attributes;
using Player.Looking.Player.Looking;
using StateMachines;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Looking
{
    [RequireComponent(typeof(GroundChecker), typeof(PlayerInput), typeof(Rigidbody))]
    public class PlayerLooking : MonoBehaviour
    {
        #region Variables

        

        private enum LeanDirection
        {
            Right = -1,
            Left = 1,
            None = 0
        }

  
        private PlayerInputActions _input;

        private Vector2 _mouseDelta;

        private LeanDirection _leanDirection = LeanDirection.None;

        private Vector2 _cameraRotation = Vector2.zero;
        
        private GroundChecker _groundChecker;
        
        private Rigidbody _rigidbody;
        
        [SerializeField] private float leanAngle = 15f;
        
        [SerializeField] private float sensitivity = 50f;
        
        [Required, ScriptableObjectDropdown] public CameraBobConfig cameraBobConfig;
        
        [Required, ScriptableObjectDropdown] public SwayConfig swayConfig;
        
        [Header("Transforms")] 
        
        [SerializeField, Required] private Transform cameraHolder;

        [SerializeField, Required] private Transform orientation;

        [SerializeField, Required] private Transform leanPoint;

        [SerializeField, Required] private Transform cameraTransform;
        
        #endregion

        #region UnityFunctions

            private void Awake()
            {
        
                
                
                InitializeControls();

                _groundChecker = GetComponent<GroundChecker>();
                
                _initialPosition = cameraTransform.localPosition;
                
           
            }
            
        #endregion



        #region Controls

        private void InitializeControls()
        {
            _input = new PlayerInputActions();
            _input.Player.Enable();
            _input.Player.Look.performed += OnMouseMove;
            _input.Player.Look.canceled += OnMouseMove;

            _input.Player.Move.performed += OnMove;
            _input.Player.Move.canceled += OnMove;
            
            _input.Player.Lean.started += OnLean;
            _input.Player.Lean.canceled += OnLean;
        }

        private Vector2 _keyboardDelta;
        private void OnMove(InputAction.CallbackContext obj)
        {
            _keyboardDelta = obj.ReadValue<Vector2>();
        }

        private void OnLean(InputAction.CallbackContext context)
        {
            _leanDirection = (LeanDirection)context.ReadValue<float>();
            Logger.Log($"Lean Direction: {_leanDirection}");
     
        }

        public void OnMouseMove(InputAction.CallbackContext context)
        {
            _mouseDelta = context.ReadValue<Vector2>();
        }

        #endregion



        #region StateMachineFunctions


        public void Lean()
        {
  
            leanPoint.rotation = Quaternion.Euler(0, _cameraRotation.y, -(float)_leanDirection * leanAngle);
            cameraHolder.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
        }
        
        public void StopLean()
        {
            leanPoint.rotation = Quaternion.Euler(0, 0, 0);
        }

        public void Look()
        {
            
            var mouseX = _mouseDelta.x * Time.deltaTime * sensitivity;
            var mouseY = _mouseDelta.y * Time.deltaTime * sensitivity;

            _cameraRotation.y += mouseX;
            _cameraRotation.x -= mouseY;

            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, -90f, 90f);

            cameraHolder.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
            orientation.rotation = Quaternion.Euler(0, _cameraRotation.y, 0);
       
        }


        private Vector3 _initialPosition;
        private float bobTimer;
        public void CameraBob(CameraBobSetting cameraBobSetting)
        {
     
            // Check if player is moving
            Vector3 velocity = _rigidbody.linearVelocity;
            float horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            bool isMoving = horizontalSpeed > 0.1f;
        
            if (isMoving)
            {
                // Increment timer based on speed
                bobTimer += Time.deltaTime * cameraBobSetting.frequency * Mathf.Min(horizontalSpeed, cameraBobSetting.maxSpeed);
            
                // Calculate bob offset using sine waves
                float horizontalBob = Mathf.Sin(bobTimer) * cameraBobSetting.horizontalAmplitude;
                float verticalBob = Mathf.Sin(bobTimer * 2) * cameraBobSetting.verticalAmplitude; // Double frequency for realistic bob
            
                // Apply speed multiplier
                float speedMultiplier = Mathf.Min(horizontalSpeed / cameraBobSetting.speedCurve, 1f);
            
                Vector3 bobOffset = new Vector3(
                    horizontalBob * speedMultiplier,
                    verticalBob * speedMultiplier,
                    0
                );
            
                cameraTransform.localPosition = _initialPosition + bobOffset;
            }
            else
            {
                // Smoothly return to initial position when stopped
                cameraTransform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition, Time.deltaTime * 4f);
                bobTimer = 0f;
            }
        }
        
        
        public void Sway(Sway sway)
        {
            cameraTransform.localPosition =
                new Vector3( Mathf.Cos(Time.time * sway.horizontalSwaySpeed) * sway.horizontalSwayAmount * sway.swayMultiplier
                    ,  Mathf.Sin(Time.time * sway.verticalSwaySpeed) * sway.verticalSwayAmount * sway.swayMultiplier, cameraHolder.transform.localPosition.z);
        }
        
   
        
        #endregion
    }
}