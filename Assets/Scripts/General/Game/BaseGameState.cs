using DependencyInjection;
using EventBus;
using General.Logging;
using Levels;

using StateMachine;

namespace General.Game
{
    public abstract class BaseGameState : BaseState
    {

        protected GameManager GameManager;
        protected ILogger Logger;

        protected BaseGameState(GameManager gameManager)
        {
            GameManager = gameManager;
            RuntimeResolver.Instance.TryResolve(out Logger);
        }
    }
}