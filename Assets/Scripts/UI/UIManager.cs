using EventBus;
using General.Game;
using StateMachine;
using UI.States;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIManager : MonoBehaviour
    {
        private UIState _currentMenuState;
        private UIDocument _document;

        private EventBinding<GameStateChangedEvent> _gameStateChanged;

        private IState _inGameState;
        private IState _levelSelectState;
        private IState _mainMenuState;
        private IState _pausedState;
        private VisualElement _root;
        private IState _settingsState;
        private StateMachine.StateMachine _stateMachine;


        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _root = _document.rootVisualElement;

            EventBus<GameStateChangedEvent>.Register(_gameStateChanged);

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


        public void EnableSettingsMenu()
        {
            _currentMenuState = UIState.Settings;
        }

        public void EnableInGameMenu()
        {
            _currentMenuState = UIState.InGame;
        }

        public void EnablePausedMenu()
        {
            _currentMenuState = UIState.GamePaused;
        }

        public void EnableMainMenu()
        {
            _currentMenuState = UIState.MainMenu;
        }

        public void InitializeStateMachine()
        {
            _stateMachine = new StateMachine.StateMachine();

            _inGameState = new InGameUIState(_root.Q<VisualElement>("InGameRoot"));
            _pausedState = new PausedUIState(_root.Q<VisualElement>("PausedRoot"), ResumeButtonClicked,
                SettingsButtonClicked);
            _settingsState = new SettingsUIState(_root.Q<VisualElement>("SettingsRoot"));
            _mainMenuState = new MainMenuUIState(_root.Q<VisualElement>("MainMenuRoot"));
            _levelSelectState = new LevelSelectUIState(_root.Q<VisualElement>("LevelSelectRoot"));


            _stateMachine.SetState(_inGameState);
        }

        private void SettingsButtonClicked()
        {
            _stateMachine.SetState(_settingsState);
        }

        private void ResumeButtonClicked()
        {
            _stateMachine.SetState(_inGameState);
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

    public class LevelSelectUIState : UIBaseState
    {
        public LevelSelectUIState(VisualElement rootElement) : base(rootElement)
        {
        }
    }

    public class MainMenuUIState : UIBaseState
    {
        public MainMenuUIState(VisualElement rootElement) : base(rootElement)
        {
        }
    }
}