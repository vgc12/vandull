using UI.States;
using UnityEngine.UIElements;

namespace UI
{
    public class InGameSettingsUIState : SettingsUIState
    {
        public InGameSettingsUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement,
            stateMachine, UIStateType.InGameSettings)
        {
        }
    }
}