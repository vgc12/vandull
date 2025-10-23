using UnityEngine.UIElements;

namespace UI
{
    public class MissionSuccessUIState : MissionOverUIState
    {
        public MissionSuccessUIState(VisualElement rootElement, Data stateData) : base(rootElement, stateData)
        {
        }

        protected override void ChangeMouseState()
        {
            UnlockCursorAndShowMouse();
        }
    }
}