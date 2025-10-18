using System.Collections.Generic;
using Attributes;
using EventBus;
using Levels.Strategies;
using Npcs;
using Singletons;
using UnityEngine.SceneManagement;

namespace Levels
{
    public class LevelManager : Singleton<LevelManager>
    {
        [ScriptableObjectDropdown] public List<LevelConfig> levels;

        private LevelConfig _currentLevel;
        private EventBinding<EnemyKilledEvent> _enemyKilledEventBinding;


        private EventBinding<LevelLoadEvent> _levelSelectedEventBinding;
        private IMissionStrategy _missionStrategy;

        private int _remainingEnemies;


        protected override void Awake()
        {
            base.Awake();
            levels ??= new List<LevelConfig>();
            _currentLevel ??= levels[0];
            _levelSelectedEventBinding = new EventBinding<LevelLoadEvent>(OnLevelShouldLoad);
            EventBus<LevelLoadEvent>.Register(_levelSelectedEventBinding);
            LoadLevel(_currentLevel);

            _enemyKilledEventBinding = new EventBinding<EnemyKilledEvent>(OnEnemyKilled);

            EventBus<EnemyKilledEvent>.Register(_enemyKilledEventBinding);

            _remainingEnemies = _currentLevel.enemyCount;
        }

        private void OnDisable()
        {
            EventBus<LevelLoadEvent>.Deregister(_levelSelectedEventBinding);
            EventBus<EnemyKilledEvent>.Deregister(_enemyKilledEventBinding);
        }

        private void OnDestroy()
        {
        }


        public void Initialize(LevelConfig config)
        {
        }

        public void OnEnemyKilled()
        {
            _remainingEnemies--;
            EventBus<LevelEvent>.Raise(new LevelEvent(LevelEventType.EnemyKilled, _remainingEnemies));
            if (_remainingEnemies <= 0)
                EventBus<LevelEvent>.Raise(new LevelEvent(LevelEventType.LevelWon));
        }

        public void OnHostageRescued()
        {
        }

        public void OnBombDefused()
        {
        }


        public void Cleanup()
        {
            EventBus<EnemyKilledEvent>.Deregister(_enemyKilledEventBinding);
        }


        private void OnLevelShouldLoad(LevelLoadEvent obj)
        {
            LoadLevel(obj.LevelConfig);
        }

        private void LoadLevel(LevelConfig config)
        {
            _currentLevel = config;
        }

        public void ReloadLevel()
        {
            SceneManager.LoadScene(_currentLevel.levelName);
        }
    }
}