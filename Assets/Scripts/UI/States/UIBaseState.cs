using StateMachine;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

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
            ChangeMouseState();
        }


        public override void Exit()
        {
            RootPageElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        ///     Called on enter, used to change whether the mouse is locked or not
        /// </summary>
        protected virtual void ChangeMouseState()
        {
            UnlockCursorAndShowMouse();
        }

        protected static void LockCursorAndHideMouse()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }

        protected static void UnlockCursorAndShowMouse()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}