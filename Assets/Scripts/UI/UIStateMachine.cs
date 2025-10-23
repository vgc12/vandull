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
        private UIState _currentMenuState;
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

        private VisualElement _root;
        private StateMachine.StateMachine _stateMachine;


        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _root = _document.rootVisualElement;

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

            _inGameState = new InGameUIState(_root.Q<VisualElement>("InGame"));
            var settingsElement = _root.Q<VisualElement>("settings-root");
            _inGameSettingsState =
                new InGameSettingsUIState(settingsElement, () => _stateMachine.ChangeState(_pausedState));
            _mainMenuState =
                new MainMenuUISettingsState(settingsElement, () => _stateMachine.ChangeState(_mainMenuState));
            /*  _pausedState = new PausedUIState(_root.Q<VisualElement>("PausedRoot"), ResumeButtonClicked,
                  SettingsButtonClicked, QuitButtonClicked);

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
                new MissionSuccessUIState(_root.Q<VisualElement>("mission-over-root"), missionWonData.Build());

            builder = new MissionOverUIState.Data.Builder();
            var missionLostData = builder
                .WithStatusLabelText("Mission Failed")
                .WithDescriptionLabelText("You have been defeated.")
                .WithStatusLabelColor(Color.red)
                .WithDescriptionLabelColor(Color.white);

            _missionLostState =
                new MissionFailedUIState(_root.Q<VisualElement>("mission-over-root"), missionLostData.Build());

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


        private void PauseSettingsButtonClicked()
        {
            _stateMachine.ChangeState(_inGameSettingsState);
        }

        private void MainMenuSettingsButtonClicked()
        {
            _stateMachine.ChangeState(_mainMenuSettingsState);
        }

        private void ResumeButtonClicked()
        {
            _stateMachine.ChangeState(_inGameState);
        }

        private void SettingsBackButtonClicked()
        {
        }

        private void QuitButtonClicked()
        {
            _stateMachine.ChangeState(_quitMenuState);
        }

        private enum UIState
        {
            InGame,
            GamePaused,
            Settings,
            LevelSelect,
            MainMenu
        }
    }
}