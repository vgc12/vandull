using EventBus;
using General;
using Npcs;
using Singletons;
using StateMachine;
using UI.States;

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
        private IState _pauseState;

        private UIStateType _uiStateType;

        private EventBinding<UIStateSwitchedEvent> uiStateSwitchedEventBinding;

        private StateMachine.StateMachine StateMachine { get; set; }

        protected override void Awake()
        {
            base.Awake();
            StateMachine = new StateMachine.StateMachine();
            _mainMenuState = new MainMenuGameState(this);
            _loadingState = new LoadingGameState(this);
            _pauseState = new PauseGameState(this);
            _inGame = new InGameState(this);
            _levelLost = new LevelLostGameState(this);
            _levelWon = new LevelWonGameState(this);
            uiStateSwitchedEventBinding = new EventBinding<UIStateSwitchedEvent>(OnUISwitched);
            EventBus<UIStateSwitchedEvent>.Register(uiStateSwitchedEventBinding);

            StateMachine.AddTransition(_mainMenuState, _loadingState, () => _uiStateType == UIStateType.Loading);
            StateMachine.AddTransition(_m);
        }

        private void OnUISwitched(UIStateSwitchedEvent obj)
        {
            _uiStateType = obj.NewState;
        }


        public void ChangeState<T>() where T : BaseGameState
        {
            StateMachine.ChangeState(typeof(T));
        }
    }
    // possibly delete

    public enum GameState
    {
        MainMenu,
        InGame,
        Loading,
        LevelLost,
        LevelWon,
        Paused
    }

    public class GameStateEvent : IEvent
    {
        public GameStateEvent(GameState newState)
        {
            NewState = newState;
        }

        public GameState NewState { get; init; }
    }
}