using EventBus;
using General.Game;
using UI.States;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIManager : MonoBehaviour
    {
        private GameState _currentGameState;

        private UIState _currentMenuState;
        private UIDocument _document;

        private EventBinding<GameStateChangedEvent> _gameStateChanged;
        private VisualElement _root;
        private StateMachine.StateMachine _stateMachine;


        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            _root = _document.rootVisualElement;

            _gameStateChanged = new EventBinding<GameStateChangedEvent>(OnGameStateChanged);
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

        private void OnGameStateChanged(GameStateChangedEvent obj)
        {
            _currentGameState = obj.NewGameState;
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
            var inGameState = new InGameUIState(_root.Q<VisualElement>("InGameRoot"));
            var pausedState = new PausedUIState(_root.Q<VisualElement>("PausedRoot"));
            var settingsState = new SettingsUIState(_root.Q<VisualElement>("SettingsRoot"));

            _stateMachine = new StateMachine.StateMachine();
            _stateMachine.AddTransition(inGameState, pausedState,
                () => _currentMenuState is UIState.GamePaused);
            _stateMachine.AddTransition(pausedState, settingsState,
                () => _currentMenuState is UIState.Settings);
            _stateMachine.AddTransition(settingsState, pausedState,
                () => _currentMenuState is UIState.GamePaused);
            _stateMachine.AddTransition(pausedState, inGameState,
                () => _currentMenuState is UIState.InGame);
            _stateMachine.SetState(inGameState);
        }

        private enum UIState
        {
            InGame,
            GamePaused,
            Settings,
            MainMenu
        }
    }
}