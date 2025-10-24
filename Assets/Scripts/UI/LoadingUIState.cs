using UI.States;
using UnityEngine.UIElements;

namespace UI
{
    public class LoadingUIState : UIBaseState
    {
        public LoadingUIState(VisualElement rootElement, UIStateMachine uiStateMachine) : base(rootElement, uiStateMachine, UIStateType.Loading)
        {
        }
    }
}