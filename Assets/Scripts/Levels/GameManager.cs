using System.Collections.Generic;
using EventBus;
using General.Game;
using Levels.Strategies;
using Npcs;
using Singletons;
using StateMachine;
using UI.States;

namespace Levels
{
    /// <summary>
    ///     GameManager: Handles global game state, pause, time scale, and cross-level concerns
    /// </summary>
    public class GameManager : PersistentSingleton<GameManager>
    {
        private ISpawner<Enemy> _enemySpawner;
        private IState _inGame;
        private bool _levelLost;

        private EventBinding<LevelLostEvent> _levelLostEventBinding;
        private IState _levelOver;


        private bool _levelWon;
        private EventBinding<LevelWonEvent> _levelWonEventBinding;
        private IState _loadingState;
        private IState _mainMenuState;
        private IState _pauseState;
        private IState _playerCaughtState;
        private EventBinding<PlayerKilledEvent> _playerKilledEventBinding;

        private EventBinding<UIStateSwitchedEvent> _uiStateSwitchedEventBinding;

        private UIStateType _uiStateType;
        private EventBinding<PlayerDetectionChangedEvent> _playerDetectionChangedEventBinding;

        private StateMachine.StateMachine StateMachine { get; set; }

        public bool PlayerCaught { get; private set; }
        public List<Enemy> EnemiesThatSeePlayer { get; } = new List<Enemy>();

        protected override void Awake()
        {
            base.Awake();
            StateMachine = new StateMachine.StateMachine();
            _mainMenuState = new MainMenuGameState(this);
            _loadingState = new LoadingGameState(this);
            _pauseState = new PauseGameState(this);
            _inGame = new InGameState(this);
            _levelOver = new LevelOverGameState(this);
            _playerCaughtState = new PlayerCaughtGameState(this);


            _playerDetectionChangedEventBinding = new EventBinding<PlayerDetectionChangedEvent>(OnPlayerDetectionChanged);
            _uiStateSwitchedEventBinding = new EventBinding<UIStateSwitchedEvent>(OnUISwitched);

            _levelLostEventBinding = new EventBinding<LevelLostEvent>(OnLevelLost);
            _levelWonEventBinding = new EventBinding<LevelWonEvent>(OnLevelWon);

            EventBus<LevelWonEvent>.Register(_levelWonEventBinding);
            EventBus<LevelLostEvent>.Register(_levelLostEventBinding);
            EventBus<PlayerDetectionChangedEvent>.Register(_playerDetectionChangedEventBinding);
            EventBus<UIStateSwitchedEvent>.Register(_uiStateSwitchedEventBinding);


            StateMachine.AddAnyTransition(_loadingState, () => LevelManager.Instance.IsLoading);
            StateMachine.AddTransition(_loadingState, _mainMenuState,
                () => !LevelManager.Instance.IsLoading && _uiStateType == UIStateType.MainMenu);
            StateMachine.AddTransition(_loadingState, _inGame,
                () => !LevelManager.Instance.IsLoading && _uiStateType == UIStateType.InGame);

            StateMachine.AddTransition(_inGame, _pauseState, () => _uiStateType == UIStateType.Paused);
            StateMachine.AddTransition(_inGame, _playerCaughtState, () => PlayerCaught);
            
            StateMachine.AddTransition(_pauseState, _inGame, () => _uiStateType == UIStateType.InGame);

            
            StateMachine.AddTransition(_pauseState, _levelOver, () => _levelWon || _levelLost);
            StateMachine.AddTransition(_inGame, _levelOver, () => _levelWon || _levelLost);

            
            StateMachine.AddTransition(_levelOver, _mainMenuState, () => _uiStateType == UIStateType.MainMenu);

            StateMachine.SetState(_inGame);
        }

        private void OnPlayerDetectionChanged(PlayerDetectionChangedEvent obj)
        {
            if (obj.Detected)
            {
                if (!EnemiesThatSeePlayer.Contains(obj.Enemy))
                    EnemiesThatSeePlayer.Add(obj.Enemy);
            }
            else
            {
                if (EnemiesThatSeePlayer.Contains(obj.Enemy))
                    EnemiesThatSeePlayer.Remove(obj.Enemy);
            }

            PlayerCaught = EnemiesThatSeePlayer.Count > 0;
        }

        private void OnLevelWon(LevelWonEvent obj)
        {
            _levelWon = true;
        }

        private void OnLevelLost(LevelLostEvent obj)
        {
            _levelLost = true;
        }


        private void OnUISwitched(UIStateSwitchedEvent obj)
        {
            _uiStateType = obj.NewState;
        }
    }

    public class PlayerCaughtGameState : BaseGameState
    {
        public PlayerCaughtGameState(GameManager gameManager) : base(gameManager)
        {
        }
    }
}