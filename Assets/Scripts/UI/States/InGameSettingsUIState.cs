using UnityEngine.UIElements;

namespace UI.States
{
    public class InGameSettingsUIState : SettingsUIState
    {
        public InGameSettingsUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement,
            stateMachine, UIStateType.InGameSettings)
        {
        }
    }
}