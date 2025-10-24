using System;
using UnityEngine.UIElements;

namespace UI.States
{
    public class QuitUIState : UIBaseState
    {
        private readonly Button _quitToDesktopButton;
        private readonly Action _quitToDesktopClicked;
        private readonly Button _quitToMenuButton;

        private readonly Action _quitToMenuClicked;

        public QuitUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.Quit)
        {
            _quitToMenuButton = rootElement.Q<Button>("quit-to-menu-button");
            _quitToDesktopButton = rootElement.Q<Button>("quit-to-desktop-button");

            if (_quitToMenuButton != null) _quitToMenuButton.clicked += stateMachine.QuitToMenuButtonClicked;
            if (_quitToDesktopButton != null) _quitToDesktopButton.clicked += stateMachine.QuitToDesktopButtonClicked;
        }

        ~QuitUIState()
        {
            if (_quitToMenuButton != null) _quitToMenuButton.clicked -= _quitToMenuClicked;
            if (_quitToDesktopButton != null) _quitToDesktopButton.clicked -= _quitToDesktopClicked;
        }
    }
}