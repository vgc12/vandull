using StateMachines;
using UnityEngine;

namespace Player.Movement
{
    public class JumpState : BaseState
    {
        private readonly PlayerMovement _playerMovement;
        
        public JumpState(PlayerMovement playerMovement)
        {
            _playerMovement = playerMovement;
        }

        public override void Enter()
        {
            
            _playerMovement.Jump();
        }
        
        
        public override void Update()
        {
            _playerMovement.ApplyDrag();
            _playerMovement.CheckForGround();
        }
    }
}