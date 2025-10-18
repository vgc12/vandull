using System;
using UI.States;
using UnityEngine.UIElements;

namespace UI
{
    public class QuitUIState : UIBaseState
    {
        private readonly Button _quitToDesktopButton;
        private readonly Action _quitToDesktopClicked;
        private readonly Button _quitToMenuButton;

        private readonly Action _quitToMenuClicked;

        public QuitUIState(VisualElement rootElement, Action quitToMenuClicked = null,
            Action quitToDesktopClicked = null) : base(rootElement)
        {
            _quitToMenuButton = rootElement.Q<Button>("QuitToMenuButton");
            _quitToDesktopButton = rootElement.Q<Button>("QuitToDesktopButton");

            _quitToMenuClicked = quitToMenuClicked;
            _quitToDesktopClicked = quitToDesktopClicked;

            if (_quitToMenuButton != null) _quitToMenuButton.clicked += _quitToMenuClicked;
            if (_quitToDesktopButton != null) _quitToDesktopButton.clicked += _quitToDesktopClicked;
        }

        ~QuitUIState()
        {
            if (_quitToMenuButton != null) _quitToMenuButton.clicked -= _quitToMenuClicked;
            if (_quitToDesktopButton != null) _quitToDesktopButton.clicked -= _quitToDesktopClicked;
        }
    }
}