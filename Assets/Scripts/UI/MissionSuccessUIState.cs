using UnityEngine.UIElements;

namespace UI
{
    public class MissionSuccessUIState : MissionOverUIState
    {
        public MissionSuccessUIState(VisualElement rootElement, Data stateStateData) : base(rootElement, stateStateData)
        {
        }

        protected override void ChangeMouseState()
        {
            UnlockCursorAndShowMouse();
        }
    }
}