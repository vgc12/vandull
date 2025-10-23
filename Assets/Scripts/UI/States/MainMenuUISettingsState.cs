using UI.States;
using UnityEngine.UIElements;

namespace UI
{
    public class MainMenuUISettingsState : SettingsUIState
    {
        public MainMenuUISettingsState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement,
            stateMachine, UIStateType.MainMenuSettings)
        {
        }
    }
}