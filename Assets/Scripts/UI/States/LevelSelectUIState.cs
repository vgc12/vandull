using EventBus;
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
                if (level.levelName == "Main Menu") continue;
                var button = new Button(() => EventBus<LevelLoadEvent>.Raise(new LevelLoadEvent(level)))
                {
                    text = level.levelName
                };
                button.AddToClassList("button-red");
                button.AddToClassList("level-select-button");
                Logger.Log($"Button classes: {string.Join(", ", button.GetClasses())}");
                container.Add(button);
            }
        }
    }
}