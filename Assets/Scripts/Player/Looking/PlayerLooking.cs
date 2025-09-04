using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Attributes;
using Player.PlayerLooking.States;
using StateMachines;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.PlayerLooking
{
    
    public class PlayerLooking : MonoBehaviour
    {
        #region Variables

        

        private enum LeanDirection
        {
            Right = -1,
            Left = 1,
            None = 0
        }

        private StateMachine _stateMachine;
        private PlayerInputActions _input;

        private Vector2 _mouseDelta;

        private LeanDirection _leanDirection = LeanDirection.None;

        private Vector2 _cameraRotation = Vector2.zero;
        
        
        [SerializeField] private float leanAngle = 15f;
        [SerializeField] private float sensitivity = 50f;

        [Header("Transforms")] 
        
        [SerializeField, Required] private Transform cameraHolder;

        [SerializeField, Required] private Transform orientation;

        [SerializeField, Required] private Transform leanPoint;
        
        #endregion

        #region UnityFunctions


            private void Awake()
            {
        
                
                InitializeControls();

                _stateMachine = new StateMachine();

                var standingState = new StandingState(this);
                var leaningState = new LeaningState(this);

                _stateMachine.AddTransition(standingState, leaningState,
                    new FuncPredicate(() => _leanDirection != LeanDirection.None));
                _stateMachine.AddTransition(leaningState, standingState, 
                    new FuncPredicate(() => _leanDirection == LeanDirection.None));

                _stateMachine.SetState(standingState);
            }
            
            private void Update()
            {
                _stateMachine.Update();
            }
            
            private void FixedUpdate()
            {
                _stateMachine.FixedUpdate();
            }
        #endregion



        #region Controls

        private void InitializeControls()
        {
            _input = new PlayerInputActions();
            _input.Player.Enable();
            _input.Player.Look.performed += OnMouseMove;
            _input.Player.Look.canceled += OnMouseMove;

            _input.Player.Lean.started += OnLean;
            _input.Player.Lean.canceled += OnLean;
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
            //leanPoint.Rotate(orientation.forward, -(float)_leanDirection * leanAngle);
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
        
        
        #endregion
    }
}