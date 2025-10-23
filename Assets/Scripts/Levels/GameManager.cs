using EventBus;
using General;
using Npcs;
using Singletons;
using StateMachine;

namespace Levels
{
    /// <summary>
    ///     GameManager: Handles global game state, pause, time scale, and cross-level concerns
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        private ISpawner<Enemy> _enemySpawner;
        private IState _inGame;
        private IState _levelLost;
        private IState _levelWon;
        private IState _loadingState;
        private IState _mainMenuState;

        public StateMachine.StateMachine StateMachine { get; private set; }


        protected override void Awake()
        {
            base.Awake();
            StateMachine = new StateMachine.StateMachine();
            _mainMenuState = new MainMenuGameState(this);
            _loadingState = new LoadingGameState(this);
            _inGame = new InGameState(this);
            _levelLost = new LevelLostGameState(this);
            _levelWon = new LevelWonGameState(this);
        }

        public void ChangeState<T>() where T : BaseGameState
        {
            StateMachine.ChangeState(typeof(T));
        }
    }

    public class GameStateEvent : IEvent
    {
    }
}