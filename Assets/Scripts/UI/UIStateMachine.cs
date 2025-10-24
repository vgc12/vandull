using System;
using EventBus;
using Levels;
using Levels.Strategies;
using Player.Input;
using Reflex.Attributes;
using StateMachine;
using UI.States;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIStateMachine : MonoBehaviour
    {
        private UIDocument _document;
        private IState _inGameSettingsState;
        private IState _inGameState;

        private EventBinding<LevelLostEvent> _levelLostEventBinding;
        private IState _levelSelectState;
        private EventBinding<LevelWonEvent> _levelWonEventBinding;
        private IState _mainMenuSettingsState;

        private IState _mainMenuState;
        private IState _missionLostState;
        private IState _missionWonState;
        private IState _pausedState;
        private IState _quitMenuState;
        private IState _loadingState;
        
        private StateMachine.StateMachine _stateMachine;

        private bool _backPressed;
        private bool _settingsPressed;
        private bool _quitButtonClicked;
        private bool _quitToMenuButtonClicked;
        private bool _isLoading;
        private bool _levelWon;
        private bool _levelLost;
        
        public VisualElement Root { get; private set; }


        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            Root = _document.rootVisualElement;
            
            _levelLostEventBinding = new EventBinding<LevelLostEvent>(LevelLostEvent);
            _levelWonEventBinding = new EventBinding<LevelWonEvent>(LevelWonEvent);
            EventBus<LevelLostEvent>.Register(_levelLostEventBinding);
            EventBus<LevelWonEvent>.Register(_levelWonEventBinding);

            InitializeStateMachine();
        }

        private void Start()
        {
            _input.InGameCancel += () => _backPressed = true;
            _input.InMenuCancel += () => _backPressed = true;
        }
        
        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        private void LevelWonEvent(LevelWonEvent obj)
        {
            _levelWon = true;
        }


        private void LevelLostEvent(LevelLostEvent obj)
        {
           _levelLost = true;
        }

        [Inject]
        private IInputService _input;
        
        


        public void InitializeStateMachine()
        {
        
            
            _stateMachine = new StateMachine.StateMachine();

            _loadingState = new LoadingUIState(Root.Q<VisualElement>("loading"), this);
            
            _inGameState = new InGameUIState(Root.Q<VisualElement>("in-game"), this);
            var settingsElement = Root.Q<VisualElement>("settings");
            _inGameSettingsState = new InGameSettingsUIState(settingsElement, this);
            _mainMenuSettingsState = new MainMenuUISettingsState(settingsElement, this);
            _pausedState = new PausedUIState(Root.Q<VisualElement>("paused"), this);
            
              _mainMenuState = new MainMenuUIState(Root.Q<VisualElement>("main-menu"), this);
              _levelSelectState = new LevelSelectUIState(Root.Q<VisualElement>("level-select"), this);
              _quitMenuState = new QuitUIState(Root.Q<VisualElement>("quit"), this);
             
            var builder = new MissionOverUIState.Data.Builder();
            var missionWonData = builder
                .WithStatusLabelText("Mission Accomplished")
                .WithDescriptionLabelText("All objectives completed successfully.")
                .WithStatusLabelColor(Color.green)
                .WithDescriptionLabelColor(Color.white);

            _missionWonState =
                new MissionSuccessUIState(Root.Q<VisualElement>("mission-over"), this, missionWonData.Build());

            builder = new MissionOverUIState.Data.Builder();
            var missionLostData = builder
                .WithStatusLabelText("Mission Failed")
                .WithDescriptionLabelText("You have been defeated.")
                .WithStatusLabelColor(Color.red)
                .WithDescriptionLabelColor(Color.white);

            _missionLostState =
                new MissionFailedUIState(Root.Q<VisualElement>("mission-over"), this, missionLostData.Build());

            _stateMachine.AddAnyTransition(_loadingState, () => LevelManager.Instance.IsLoading);
            
            _stateMachine.AddTransition(_loadingState, _inGameState, () => !LevelManager.Instance.IsLoading && LevelManager.Instance.IsLevelActive);
            
            _stateMachine.AddTransition(_inGameState, _pausedState, () => _backPressed);
            _stateMachine.AddTransition(_inGameState, _missionWonState, () => _levelWon);
            _stateMachine.AddTransition(_inGameState, _missionLostState, () => _levelLost);
            
            _stateMachine.AddTransition(_mainMenuSettingsState, _mainMenuState, () => _backPressed);
            
            _stateMachine.AddTransition(_inGameSettingsState, _pausedState, () => _backPressed);
            
            _stateMachine.AddTransition(_pausedState, _inGameSettingsState, () => _settingsPressed);
            _stateMachine.AddTransition(_pausedState, _quitMenuState, () => _quitButtonClicked);
            _stateMachine.AddTransition(_pausedState, _inGameState, () => _backPressed);
            
            _stateMachine.AddTransition(_mainMenuState, _mainMenuSettingsState, () => _settingsPressed);
            _stateMachine.AddTransition(_mainMenuState, _quitMenuState, () => _quitButtonClicked);
            
            _stateMachine.AddTransition(_quitMenuState, _pausedState, () => _backPressed && LevelManager.Instance.IsLevelActive);
            _stateMachine.AddTransition(_quitMenuState, _mainMenuState, () => _quitToMenuButtonClicked && !LevelManager.Instance.IsLevelActive);
            
          _stateMachine.AddState(_levelSelectState);
            _stateMachine.ChangeState(_levelSelectState);
        }


        public void PauseSettingsButtonClicked()
        {
            _settingsPressed = true;
        }

        public void MainMenuSettingsButtonClicked()
        {
            _settingsPressed = true;
        }

        public void ResumeButtonClicked()
        {
            _backPressed = true;
        }

        public void SettingsBackButtonClicked()
        {
            _backPressed = true;
        }
        

    

        public void QuitToMenuButtonClicked()
        {
            _quitToMenuButtonClicked = true;
        }

        public void QuitToDesktopButtonClicked()
        {
            Application.Quit();
        }

        public void QuitButtonClicked()
        {
            _quitButtonClicked = true;
        }

        public void ResetInteractions()
        {
            _backPressed = false;
            _settingsPressed = false;
            _quitButtonClicked = false;
        }
    }
}