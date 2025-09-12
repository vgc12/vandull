using System;
using System.Globalization;
using Attributes;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Player.Looking
{
    [RequireComponent(typeof(GroundChecker), typeof(CameraBobber), typeof(Rigidbody))]
    public class PlayerLooking : MonoBehaviour
    {
        #region Variables

        private enum LeanDirection
        {
            Right = -1,
            Left = 1,
            None = 0
        }

        [Required]  public ObjectSwayer objectSwayer;

        private PlayerInputActions _input;

        private Vector2 _mouseDelta;

        private LeanDirection _leanDirection = LeanDirection.None;
        
        private Vector2 _cameraRotation = Vector2.zero;


        private Rigidbody _rigidbody;

        public CameraBobber CameraBobber { get; private set; }

        [Header("Configuration")] [SerializeField, Required, ScriptableObjectDropdown]
        private PlayerLookingConfig config;

        [ Required, ScriptableObjectDropdown] public SwayConfig swayConfig;
   

        [Header("Transforms")] [SerializeField, Required]
        private Transform cameraHolder;

        [SerializeField, Required] private Transform orientation;

        [SerializeField, Required] private Transform leanPoint;

        #endregion

        #region UnityFunctions

        private void Awake()
        {
            InitializeControls();
        }

        private void Start()
        {
            CameraBobber = GetComponent<CameraBobber>();
        }


        private void OnDestroy()
        {
            _input.Player.Look.performed -= OnMouseMove;
            _input.Player.Look.canceled -= OnMouseMove;
            _input.Player.Lean.started -= OnLeaning;
            _input.Player.Lean.canceled -= OnLeaning;
        }


        #endregion


        #region Controls

        private void InitializeControls()
        {
            _input = new PlayerInputActions();
            _input.Player.Enable();
            _input.Player.Look.performed += OnMouseMove;
            _input.Player.Look.canceled += OnMouseMove;


            _input.Player.Lean.started += OnLeaning;
            _input.Player.Lean.canceled += OnLeaning;
        }


        private void OnLeaning(InputAction.CallbackContext context)
        {
            _leanDirection = (LeanDirection)context.ReadValue<float>();
        }

        public void OnMouseMove(InputAction.CallbackContext context)
        {
            _mouseDelta = context.ReadValue<Vector2>();
        }

        #endregion


        #region StateMachineFunctions


        public void Lean()
        {
         
            var rot = Quaternion.Slerp(leanPoint.rotation,
                Quaternion.Euler(0, _cameraRotation.y, -(float)_leanDirection * config.LeanAngle),
                config.LeanSpeed * Time.deltaTime);
            
            leanPoint.rotation = rot;
            cameraHolder.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
        }


        public void Look()
        {
            var mouseX = _mouseDelta.x * Time.deltaTime * config.Sensitivity;
            var mouseY = _mouseDelta.y * Time.deltaTime * config.Sensitivity;

            _cameraRotation.y += mouseX;
            _cameraRotation.x -= mouseY;

            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, -90f, 90f);

            cameraHolder.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
            orientation.rotation = Quaternion.Euler(0, _cameraRotation.y, 0);
        }

        #endregion
    }
    
    
    
}