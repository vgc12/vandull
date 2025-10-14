using UI.States;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class UIManager : MonoBehaviour
    {
        private UIDocument _document;
        private StateMachine.StateMachine _stateMachine;

        private void Awake()
        {
            _document = GetComponent<UIDocument>();

            InitializeStateMachine();
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        public void InitializeStateMachine()
        {
            _stateMachine = new StateMachine.StateMachine();
            var states = Factory.Create(_document.rootVisualElement);
            _stateMachine.AddAnyTransition(states.InGameUIState, () => true);
            _stateMachine.SetState(states.InGameUIState);
        }

        private class Factory
        {
            public InGameUIState InGameUIState { get; private init; }

            public static Factory Create(VisualElement rootElement)
            {
                var s = new InGameUIState(rootElement.Q<VisualElement>("InGameRoot"));
                return new Factory
                {
                    InGameUIState = s
                };
            }
        }
    }
}