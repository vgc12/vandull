using General.Game;
using UnityEngine;

namespace Levels
{
    public sealed class PauseGameState : BaseGameState
    {
        public PauseGameState(GameManager gameManager) : base(gameManager)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Time.timeScale = 0f;
        }


        public override void Exit()
        {
            base.Exit();
            Time.timeScale = 1f;
        }
    }
}