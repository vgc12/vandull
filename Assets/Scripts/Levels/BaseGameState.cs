using DependencyInjection;
using General.Logging;
using StateMachine;

namespace Levels
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