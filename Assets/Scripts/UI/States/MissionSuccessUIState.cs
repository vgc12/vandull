using UnityEngine.UIElements;

namespace UI.States
{
    public sealed class MissionSuccessUIState : MissionOverUIState
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