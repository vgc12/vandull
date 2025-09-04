using StateMachines;
using UnityEngine;

namespace Player.Movement
{
    public class WalkState : BaseState
    {
        private readonly PlayerMovement _playerMovement;
        
        public WalkState(PlayerMovement playerMovement)
        {
            _playerMovement = playerMovement;
        }

        public override void Update()
        {
            _playerMovement.ApplyDrag();
        }

        public override void FixedUpdate()
        {
            _playerMovement.Walk();
            
        }
    }
}