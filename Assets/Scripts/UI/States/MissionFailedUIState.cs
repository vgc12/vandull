using UI.States;
using UnityEngine.UIElements;

namespace UI
{
    public class MissionFailedUIState : MissionOverUIState
    {
        public MissionFailedUIState(VisualElement rootElement, UIStateMachine stateMachine, Data stateData) : base(
            rootElement, stateMachine, stateData, UIStateType.MissionFailed)
        {
        }

        protected override void ChangeMouseState()
        {
            UnlockCursorAndShowMouse();
        }
    }
}