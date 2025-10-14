using StateMachine;
using UnityEngine.UIElements;

namespace UI.States
{
    public abstract class UIBaseState : BaseState
    {
        protected readonly VisualElement RootPageElement;

        protected UIBaseState(VisualElement rootElement)
        {
            RootPageElement = rootElement;
        }

        public override void Enter()
        {
            RootPageElement.style.display = DisplayStyle.Flex;
        }

        public override void Exit()
        {
            RootPageElement.style.display = DisplayStyle.None;
        }
    }
}