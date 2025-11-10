using System;
using UnityEngine.UIElements;

namespace UI.States
{
    public class PausedUIState : UIBaseState
    {
        private readonly Action _quitButtonClicked;
        private readonly Button _quitMenu;

        private readonly Button _resumeButton;
        private readonly Action _resumeButtonClicked;
        private readonly Button _settingsButton;
        private readonly Action _settingsButtonClicked;

        public PausedUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.Paused)
        {
            _resumeButton = rootElement.Q<Button>("resume-button");
            _settingsButton = rootElement.Q<Button>("settings-button");

            _quitMenu = rootElement.Q<Button>("quit-button");
        }

        public override void Enter()
        {
            base.Enter();

            if (_resumeButton != null) _resumeButton.clicked += UIStateMachine.ResumeButtonClicked;
            if (_settingsButton != null) _settingsButton.clicked += UIStateMachine.PauseSettingsButtonClicked;
            if (_quitMenu != null) _quitMenu.clicked += UIStateMachine.QuitButtonClicked;
        }

        public override void Exit()
        {
            if (_resumeButton != null) _resumeButton.clicked += UIStateMachine.ResumeButtonClicked;
            if (_settingsButton != null) _settingsButton.clicked += UIStateMachine.PauseSettingsButtonClicked;
            if (_quitMenu != null) _quitMenu.clicked += UIStateMachine.QuitButtonClicked;

            base.Exit();
        }
    }
}