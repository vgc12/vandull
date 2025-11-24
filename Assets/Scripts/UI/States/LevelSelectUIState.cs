using System.Collections.Generic;
using EventBus;
using Levels;
using UnityEngine.UIElements;

namespace UI.States
{
    public class LevelSelectUIState : UIBaseState
    {
        private readonly List<Level> _addedLevels = new(10);

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
                if (level.LevelName == "Main Menu" || _addedLevels.Contains(level)) continue;
                var button = new Button(() => EventBus<LevelLoadEvent>.Raise(new LevelLoadEvent(level)))
                {
                    text = level.LevelName
                };
                button.AddToClassList("settings-button");
                button.AddToClassList("level-select-button");
                Logger.Log($"Button classes: {string.Join(", ", button.GetClasses())}");
                container.Add(button);
                _addedLevels.Add(level);
            }
        }
    }
}