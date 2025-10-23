using UI.States;
using UnityEngine.UIElements;

namespace UI
{
    public class MissionSuccessUIState : MissionOverUIState
    {
        public MissionSuccessUIState(VisualElement rootElement, UIStateMachine stateMachine, Data stateData) : base(
            rootElement, stateMachine, stateData, UIStateType.MissionSuccess)
        {
        }

        protected override void ChangeMouseState()
        {
            UnlockCursorAndShowMouse();
        }
    }
}