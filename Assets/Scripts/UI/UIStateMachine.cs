using EventBus;
using Levels;
using Player;
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

        private EventBinding<LevelEvent> _gameStateChangedEvent;

        private IState _inGameState;
        private IState _levelSelectState;
        private IState _mainMenuState;
        private IState _missionLostState;
        private IState _missionWonState;
        private IState _pausedState;
        private IState _quitMenuState;

        private VisualElement _root;
        private IState _settingsState;
        private StateMachine.StateMachine _stateMachine;


        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _root = _document.rootVisualElement;

            _gameStateChangedEvent = new EventBinding<LevelEvent>(OnLevelEvent);
            
            
            InitializeStateMachine();
        }

        private void OnLevelEvent(LevelEvent obj)
        {
            if (obj.EventType == LevelEventType.LevelWon)
            {
                _stateMachine.ChangeState(_missionWonState);
            }

            else if (obj.EventType == LevelEventType.LevelLost)
            {
                _stateMachine.ChangeState(_missionLostState);
            }


        }
        

        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }


        public void InitializeStateMachine()
        {
            _stateMachine = new StateMachine.StateMachine();

            _inGameState = new InGameUIState(_root.Q<VisualElement>("InGameRoot"));
          /*  _pausedState = new PausedUIState(_root.Q<VisualElement>("PausedRoot"), ResumeButtonClicked,
                SettingsButtonClicked, QuitButtonClicked);
            _settingsState = new SettingsUIState(_root.Q<VisualElement>("SettingsRoot"));
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
            
            _missionWonState = new MissionOverUIState(_root.Q<VisualElement>("MissionOverRoot"), missionWonData.Build());
            
            var missionLostData = builder
                .WithStatusLabelText("Mission Failed")
                .WithDescriptionLabelText("You have been defeated.")
                .WithStatusLabelColor(Color.red)
                .WithDescriptionLabelColor(Color.white);
            
            _missionLostState = new MissionOverUIState(_root.Q<VisualElement>("MissionOverRoot"), missionLostData.Build());

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
            _stateMachine.ChangeState(_inGameState);
        }


        private void SettingsButtonClicked()
        {
            _stateMachine.ChangeState(_settingsState);
        }

        private void ResumeButtonClicked()
        {
            _stateMachine.ChangeState(_inGameState);
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