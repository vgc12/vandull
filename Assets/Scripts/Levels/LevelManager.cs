using System.Collections.Generic;
using EventBus;
using Levels.Strategies;
using Singletons;

namespace Levels
{
    public class LevelManager : PersistentSingleton<LevelManager>
    {
        public List<LevelConfig> levels;

        private LevelConfig _currentLevel;


        private EventBinding<LevelLoadEvent> _levelSelectedEventBinding;
        private IMissionStrategy _missionStrategy;


        protected override void Awake()
        {
            base.Awake();
            levels ??= new List<LevelConfig>();
            _levelSelectedEventBinding = new EventBinding<LevelLoadEvent>(OnLevelShouldLoad);
            EventBus<LevelLoadEvent>.Register(_levelSelectedEventBinding);
        }


        private void OnDestroy()
        {
            EventBus<LevelLoadEvent>.Deregister(_levelSelectedEventBinding);
        }


        private void OnLevelShouldLoad(LevelLoadEvent obj)
        {
            _missionStrategy?.Cleanup();

            _currentLevel = obj.LevelConfig;
            _missionStrategy = obj.LevelConfig.CreateMissionStrategy();
        }
    }
}