using UnityEngine.UIElements;

namespace UI.States
{
    public class MainMenuUIState : UIBaseState
    {
        public MainMenuUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.MainMenu)
        {
        }
    }
}