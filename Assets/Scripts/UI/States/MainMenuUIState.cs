using UnityEngine.UIElements;

namespace UI.States
{
    public sealed class MainMenuUIState : UIBaseState
    {
        private readonly Button _playButton;
        private readonly Button _quitButton;
        private readonly Button _settingsButton;

        public MainMenuUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.MainMenu)
        {
            _playButton = rootElement.Q<Button>("play-button");
            _settingsButton = rootElement.Q<Button>("settings-button");
            _quitButton = rootElement.Q<Button>("quit-button");

            _playButton.clicked += stateMachine.PlayButtonClicked;
            _settingsButton.clicked += stateMachine.MainMenuSettingsButtonClicked;
            _quitButton.clicked += stateMachine.QuitButtonClicked;
        }


        ~MainMenuUIState()
        {
            _playButton.clicked -= UIStateMachine.PlayButtonClicked;
            _settingsButton.clicked -= UIStateMachine.MainMenuSettingsButtonClicked;
            _quitButton.clicked -= UIStateMachine.QuitButtonClicked;
        }
    }
}