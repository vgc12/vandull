using Levels;
using UnityEngine.UIElements;

namespace UI.States
{
    public class LevelSelectUIState : UIBaseState
    {
        public LevelSelectUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement,
            stateMachine, UIStateType.LevelSelect)
        {
        }

        public override void Enter()
        {
            base.Enter();
            var levels = LevelManager.Instance.levels;
            var container = RootPageElement.Q<VisualElement>("level-button-container");
            foreach (var level in levels)
            {
                if(level.name == "Main Menu") continue;
                var button = new Button(() => EventBus.EventBus<LevelLoadEvent>.Raise(new LevelLoadEvent(level)))
                {
                    text = level.name
                };
                button.AddToClassList("level-select-button");
                container.Add(button);
            }
        }
    }
}