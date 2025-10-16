using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;


namespace UI.States
{
    public class InGameUIState : UIBaseState
    {
        public InGameUIState(VisualElement rootElement) : base(rootElement)
        {
            
        }

        public override void Enter()
        {
            base.Enter();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void Exit()
        {
            base.Exit();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}