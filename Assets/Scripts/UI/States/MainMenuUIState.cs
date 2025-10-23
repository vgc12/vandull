using UI.States;
using UnityEngine.UIElements;

namespace UI
{
    public class MainMenuUIState : UIBaseState
    {
        public MainMenuUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.MainMenu)
        {
        }
    }
}