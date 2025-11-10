using EventBus;
using Levels;
using Levels.Strategies;
using Player.Input;
using Reflex.Attributes;
using Singletons;
using UI.States;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIStateMachine : PersistentSingleton<UIStateMachine>
    {
        private UIDocument _document;
        private UIBaseState _inGameState;

        [Inject] private IInputService _input;

        private EventBinding<LevelLostEvent> _levelLostEventBinding;
        private UIBaseState _levelSelectState;
        private EventBinding<LevelWonEvent> _levelWonEventBinding;
        private UIBaseState _loadingState;
        private UIBaseState _mainMenuSettingsState;

        private UIBaseState _mainMenuState;
        private UIBaseState _missionLostState;
        private UIBaseState _missionWonState;
        private UIBaseState _pausedState;

        // Single command queue instead of multiple booleans
        private UICommand _pendingCommand = UICommand.None;
        private UIBaseState _quitMenuState;

        private StateMachine.StateMachine _stateMachine;

        public VisualElement Root { get; private set; }


        private void Start()
        {
            _input.InGameCancel += () => ProcessCommand(UICommand.Back);
            _input.InMenuCancel += () => ProcessCommand(UICommand.Back);

            _document = GetComponent<UIDocument>();
            Root = _document.rootVisualElement;

            _levelLostEventBinding = new EventBinding<LevelLostEvent>(OnLevelLost);
            _levelWonEventBinding = new EventBinding<LevelWonEvent>(OnLevelWon);
            EventBus<LevelLostEvent>.Register(_levelLostEventBinding);
            EventBus<LevelWonEvent>.Register(_levelWonEventBinding);

            InitializeStateMachine();
        }

        private void Update()
        {
            _stateMachine.Update();
            ResetCommand();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        private void OnLevelWon(LevelWonEvent obj)
        {
            ProcessCommand(UICommand.LevelWon);
        }

        private void OnLevelLost(LevelLostEvent obj)
        {
            ProcessCommand(UICommand.LevelLost);
        }

        // Central command processor
        public void ProcessCommand(UICommand command)
        {
            _pendingCommand = command;
        }

        // Helper methods to check commands in transition conditions
        private bool IsCommand(UICommand command)
        {
            return _pendingCommand == command;
        }

        public void InitializeStateMachine()
        {
            _stateMachine = new StateMachine.StateMachine();

            _loadingState = new LoadingUIState(Root.Q<VisualElement>("loading"), this);

            _inGameState = new InGameUIState(Root.Q<VisualElement>("in-game"), this);
            var settingsElement = Root.Q<VisualElement>("settings");
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

            // Transitions using command pattern
            _stateMachine.AddAnyTransition(_loadingState, () => LevelManager.Instance.IsLoading);

            _stateMachine.AddTransition(_loadingState, _inGameState,
                () => LevelManager.Instance.IsLevelActive);

            _stateMachine.AddTransition(_inGameState, _pausedState,
                () => IsCommand(UICommand.Back));
            _stateMachine.AddTransition(_inGameState, _missionWonState,
                () => IsCommand(UICommand.LevelWon));
            _stateMachine.AddTransition(_inGameState, _missionLostState,
                () => IsCommand(UICommand.LevelLost));

            _stateMachine.AddTransition(_mainMenuSettingsState, _pausedState, () =>
                LevelManager.Instance.IsLevelActive && IsCommand(UICommand.Back) && _mainMenuSettingsState.CanExit);
            _stateMachine.AddTransition(_mainMenuSettingsState, _mainMenuState,
                () => IsCommand(UICommand.Back) && _mainMenuSettingsState.CanExit);


            _stateMachine.AddTransition(_pausedState, _mainMenuSettingsState,
                () => IsCommand(UICommand.OpenSettings));
            _stateMachine.AddTransition(_pausedState, _quitMenuState,
                () => IsCommand(UICommand.OpenQuitMenu));
            _stateMachine.AddTransition(_pausedState, _inGameState,
                () => IsCommand(UICommand.Back) || IsCommand(UICommand.Resume));

            _stateMachine.AddTransition(_mainMenuState, _mainMenuSettingsState,
                () => IsCommand(UICommand.OpenSettings));
            _stateMachine.AddTransition(_mainMenuState, _quitMenuState,
                () => IsCommand(UICommand.OpenQuitMenu));
            _stateMachine.AddTransition(_mainMenuState, _levelSelectState,
                () => IsCommand(UICommand.Play));

            _stateMachine.AddTransition(_levelSelectState, _mainMenuState, () =>
                IsCommand(UICommand.Back));

            _stateMachine.AddTransition(_quitMenuState, _pausedState,
                () => IsCommand(UICommand.Back) && LevelManager.Instance.IsLevelActive);
            _stateMachine.AddTransition(_quitMenuState, _mainMenuState,
                () => IsCommand(UICommand.Back) || IsCommand(UICommand.QuitToMenu));

            if (LevelManager.Instance.IsLevelActive)
                _stateMachine.SetStateAndEnter(_inGameState);
            else
                _stateMachine.SetStateAndEnter(_mainMenuState);
        }


        // Simplified button callbacks - they just send commands
        public void PauseSettingsButtonClicked()
        {
            ProcessCommand(UICommand.OpenSettings);
        }

        public void MainMenuSettingsButtonClicked()
        {
            ProcessCommand(UICommand.OpenSettings);
        }

        public void ResumeButtonClicked()
        {
            ProcessCommand(UICommand.Resume);
        }

        public void BackButtonClicked()
        {
            ProcessCommand(UICommand.Back);
        }

        public void QuitToMenuButtonClicked()
        {
            ProcessCommand(UICommand.QuitToMenu);
        }

        public void QuitToDesktopButtonClicked()
        {
            Application.Quit();
        }

        public void QuitButtonClicked()
        {
            ProcessCommand(UICommand.OpenQuitMenu);
        }

        public void PlayButtonClicked()
        {
            ProcessCommand(UICommand.Play);
        }

        // Called by state machine after processing transitions
        public void ResetCommand()
        {
            _pendingCommand = UICommand.None;
        }
    }
}