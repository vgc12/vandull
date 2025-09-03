using System;
using Attributes;
using Player.PlayerLooking;
using StateMachines;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using StateMachine = StateMachines.StateMachine;

namespace Player.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(InputManager))]
    public class PlayerMovement : MonoBehaviour
    {
        private StateMachine _stateMachine;

        private InputManager _inputManager;

        private Rigidbody _rigidbody;
        
        private Vector2 _moveInput;
        
        
        [SerializeField, Required] private Transform orientation;
        
        private void Awake()
        {
            _inputManager = GetComponent<InputManager>();
            
            var idleState = new IdleState(this);
            var walkState = new WalkState(this);
            
            _stateMachine = new StateMachine();
            _stateMachine.AddTransition(idleState , walkState, new FuncPredicate(() => _moveInput != Vector2.zero));
            _stateMachine.AddTransition(walkState, idleState, new FuncPredicate(() => _moveInput == Vector2.zero));
            
            _stateMachine.SetState(idleState);
            
            _rigidbody = GetComponent<Rigidbody>();
            
            
        }

        private void Update()
        {
            _stateMachine.Update();
          
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            
            _moveInput = context.ReadValue<Vector2>();
        }


        public void Move()
        {
            var forwardMovement = orientation.forward * _moveInput.y;
            var rightMovement = orientation.right * _moveInput.x;
            
            _rigidbody.AddForce(forwardMovement + rightMovement, ForceMode.Force);
        }

        
        
    }
}
