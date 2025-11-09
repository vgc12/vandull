using System;
using UnityEngine.UIElements;

namespace UI.States
{
    public class QuitUIState : UIBaseState
    {
        private readonly Button _backButton;
        private readonly Button _quitToDesktopButton;
        private readonly Button _quitToMenuButton;

        private readonly Action _quitToMenuClicked;

        public QuitUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.Quit)
        {
            _quitToMenuButton = rootElement.Q<Button>("quit-to-menu-button");
            _quitToDesktopButton = rootElement.Q<Button>("quit-to-desktop-button");
            _backButton = rootElement.Q<Button>("back-button");

            if (_quitToMenuButton != null) _quitToMenuButton.clicked += stateMachine.QuitToMenuButtonClicked;
            if (_quitToDesktopButton != null) _quitToDesktopButton.clicked += stateMachine.QuitToDesktopButtonClicked;
            if (_backButton != null) _backButton.clicked += stateMachine.BackButtonClicked;
        }

        public override void Enter()
        {
            base.Enter();
        }

        ~QuitUIState()
        {
            if (_quitToMenuButton != null) _quitToMenuButton.clicked -= UIStateMachine.QuitToMenuButtonClicked;
            if (_quitToDesktopButton != null) _quitToDesktopButton.clicked -= UIStateMachine.QuitToDesktopButtonClicked;
            if (_backButton != null) _backButton.clicked -= UIStateMachine.BackButtonClicked;
        }
    }
}