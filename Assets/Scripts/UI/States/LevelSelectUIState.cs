using UI.States;
using UnityEngine.UIElements;

namespace UI
{
    public class LevelSelectUIState : UIBaseState
    {
        public LevelSelectUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement,
            stateMachine, UIStateType.LevelSelect)
        {
        }
    }
}