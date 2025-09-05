using StateMachines;
using UnityEngine;

namespace Player.Movement
{
    public class WalkState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        
        public WalkState(PlayerStateMachine sm)
        {
            _sm = sm;
        }

        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
        }

        public override void FixedUpdate()
        {
            _sm.PlayerMovement.Walk();
            
        }
    }
}