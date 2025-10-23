using EventBus;
using Levels.Strategies;
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
        private StateMachine.StateMachine _stateMachine;

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
            _stateMachine.ChangeState(_missionWonState);
        }


        private void LevelLostEvent(LevelLostEvent obj)
        {
            _stateMachine.ChangeState(_missionLostState);
        }


        public void InitializeStateMachine()
        {
            _stateMachine = new StateMachine.StateMachine();

            _inGameState = new InGameUIState(Root.Q<VisualElement>("InGame"), this);
            var settingsElement = Root.Q<VisualElement>("settings-root");
            _inGameSettingsState =
                new InGameSettingsUIState(settingsElement, this);
            _mainMenuState = new MainMenuUISettingsState(settingsElement, this);
            _pausedState = new PausedUIState(Root.Q<VisualElement>("paused-root"), this);
/*
              _mainMenuState = new MainMenuUIState(_root.Q<VisualElement>("MainMenuRoot"));
              _levelSelectState = new LevelSelectUIState(_root.Q<VisualElement>("LevelSelectRoot"));
              _quitMenuState =
                  new QuitUIState(_root.Q<VisualElement>("QuitRoot"), QuitButtonClicked, ResumeButtonClicked);
              */
            var builder = new MissionOverUIState.Data.Builder();
            var missionWonData = builder
                .WithStatusLabelText("Mission Accomplished")
                .WithDescriptionLabelText("All objectives completed successfully.")
                .WithStatusLabelColor(Color.green)
                .WithDescriptionLabelColor(Color.white);

            _missionWonState =
                new MissionSuccessUIState(Root.Q<VisualElement>("mission-over-root"), this, missionWonData.Build());

            builder = new MissionOverUIState.Data.Builder();
            var missionLostData = builder
                .WithStatusLabelText("Mission Failed")
                .WithDescriptionLabelText("You have been defeated.")
                .WithStatusLabelColor(Color.red)
                .WithDescriptionLabelColor(Color.white);

            _missionLostState =
                new MissionFailedUIState(Root.Q<VisualElement>("mission-over-root"), this, missionLostData.Build());

            _stateMachine.AddState(_inGameState);
            /*
            _stateMachine.AddState(_pausedState);
            _stateMachine.AddState(_settingsState);
            _stateMachine.AddState(_mainMenuState);
            _stateMachine.AddState(_levelSelectState);
            _stateMachine.AddState(_quitMenuState);
            */
            _stateMachine.AddState(_missionLostState);
            _stateMachine.AddState(_missionWonState);
            _stateMachine.AddState(_inGameSettingsState);
            _stateMachine.AddState(_mainMenuSettingsState);
            _stateMachine.ChangeState(_inGameState);
        }


        public void PauseSettingsButtonClicked()
        {
            _stateMachine.ChangeState(_inGameSettingsState);
        }

        public void MainMenuSettingsButtonClicked()
        {
            _stateMachine.ChangeState(_mainMenuSettingsState);
        }

        public void ResumeButtonClicked()
        {
            _stateMachine.ChangeState(_inGameState);
        }

        public void SettingsBackButtonClicked()
        {
        }

        public void SettingsButtonClicked()
        {
        }

        public void QuitButtonClicked()
        {
            _stateMachine.ChangeState(_quitMenuState);
        }

        public void QuitToMenuButtonClicked()
        {
            _stateMachine.ChangeState(_mainMenuState);
        }

        public void QuitToDesktopButtonClicked()
        {
            Application.Quit();
        }
    }
}