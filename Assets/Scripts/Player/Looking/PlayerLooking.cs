using Attributes;
using Player.PlayerLooking.States;
using StateMachines;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Looking
{
    [RequireComponent(typeof(GroundChecker))]
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
        
        private GroundChecker _groundChecker;
        
        [SerializeField] private float leanAngle = 15f;
        
        [SerializeField] private float sensitivity = 50f;
        
        [Required] public SwayConfig swayConfig;
        
        [Header("Transforms")] 
        
        [SerializeField, Required] private Transform cameraHolder;

        [SerializeField, Required] private Transform orientation;

        [SerializeField, Required] private Transform leanPoint;
        
        #endregion

        #region UnityFunctions

            private class LookingStates
            {
                public IdleState IdleState { get; init; }
                public WalkingState WalkingState { get; init; }
                public LeaningState LeaningState { get; init; }
            }

            private void Awake()
            {
        
                
                InitializeControls();

                _groundChecker = GetComponent<GroundChecker>();
                
                _stateMachine = new StateMachine();

                var idleState = new IdleState(this);
                var leaningState = new LeaningState(this);

                _stateMachine.AddTransition(idleState, leaningState,
                    new FuncPredicate(() => _leanDirection != LeanDirection.None));
                _stateMachine.AddTransition(leaningState, idleState, 
                    new FuncPredicate(() => _leanDirection == LeanDirection.None));

                _stateMachine.SetState(idleState);
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

        public void Sway(float currentMultiplier = 1)
        {
            cameraHolder.transform.position +=
                new Vector3(Mathf.Cos(Time.time * swayConfig.horizontalSwaySpeed) * swayConfig.horizontalSwayAmount * currentMultiplier
                    ,Mathf.Sin(Time.time * swayConfig.verticalSwaySpeed) * swayConfig.verticalSwayAmount, 0);
        }
        
   
        
        #endregion
    }
}