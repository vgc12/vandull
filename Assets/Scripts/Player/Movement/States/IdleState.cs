using StateMachines;
using UnityEngine;

namespace Player.Movement
{
    public class IdleState : BaseState
    {
        private readonly PlayerMovement _playerMovement;
        
        public IdleState(PlayerMovement playerMovement)
        {
            _playerMovement = playerMovement;
        }

        
        public override void Update()
        {
            _playerMovement.ApplyDrag();
            _playerMovement.CheckForGround();
        }
    }
}