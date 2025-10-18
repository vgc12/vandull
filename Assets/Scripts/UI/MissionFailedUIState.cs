using UnityEngine.UIElements;

namespace UI
{
    public class MissionFailedUIState : MissionOverUIState
    {
        public MissionFailedUIState(VisualElement rootElement, Data stateData) : base(rootElement, stateData)
        {
        }

        protected override void ChangeMouseState()
        {
            UnlockCursorAndShowMouse();
        }
    }
}