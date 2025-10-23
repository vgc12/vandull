using DependencyInjection;
using EventBus;
using General.Logging;
using StateMachine;

namespace Levels
{
    public abstract class BaseGameState : BaseState
    {
        protected GameState _state;
        protected GameManager GameManager;
        protected ILogger Logger;

        protected BaseGameState(GameManager gameManager, GameState state)
        {
            GameManager = gameManager;
            _state = state;
            RuntimeResolver.Instance.TryResolve(out Logger);
        }

        public override void Enter()
        {
            base.Enter();
            EventBus<GameStateEvent>.Raise(new GameStateEvent(_state));
        }
    }
}