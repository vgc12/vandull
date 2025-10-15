using EventBus;
using General;
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
            _stateMachine.AddTransition(states.InGameUIState,states.PausedUIState, () => GameManager.Instance.GameState == GameState.InGame);
            _stateMachine.AddTransition(states.PausedUIState,states.InGameUIState, () => GameManager.Instance.GameState == GameState.Paused);
            
            _stateMachine.SetState(states.InGameUIState);
            
        }

        private class Factory
        {
            public InGameUIState InGameUIState { get; private init; }
            public PausedUIState PausedUIState { get; private init; }

            public SettingsUIState SettingsUIState { get; private init; }
            public static Factory Create(VisualElement rootElement)
            {
             
                return new Factory
                {
                    InGameUIState = new InGameUIState(rootElement.Q<VisualElement>("InGameRoot")),
                    PausedUIState = new PausedUIState(rootElement.Q<VisualElement>("PausedRoot")),
                    SettingsUIState = new SettingsUIState(rootElement.Q<VisualElement>("SettingsRoot"))
                };
            }
        }
    }
}

