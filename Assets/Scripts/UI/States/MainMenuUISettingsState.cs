using UnityEngine.UIElements;

namespace UI.States
{
    public sealed class MainMenuUISettingsState : SettingsUIState
    {
        public MainMenuUISettingsState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement,
            stateMachine, UIStateType.MainMenuSettings)
        {
        }
    }
}