using System.Collections;
using System.Collections.Generic;
using Attributes;
using EventBus;
using General;
using General.Game;
using Levels.Strategies;
using Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Levels
{
    /// <summary>
    ///     LevelManager: Handles level loading, level-specific logic, and mission objectives
    ///     Does NOT handle pause/game state - that's GameManager's job
    /// </summary>
    public class LevelManager : PersistentSingleton<LevelManager>
    {
        [ScriptableObjectDropdown] public List<LevelConfig> levels;

        
        
        
        private LevelConfig _currentLevel;


        public bool IsLevelActive { get; private set; }

        private EventBinding<LevelLoadEvent> _levelLoadBinding;
        private int _remainingBombs;

        private int _remainingEnemies;
        private int _remainingHostages;

        private bool LevelWon => _remainingEnemies <= 0 && _remainingHostages <= 0 && _remainingBombs <= 0;
        public bool IsLoading { get; private set; }


        protected override void Awake()
        {
            base.Awake();

            levels ??= new List<LevelConfig>();

            // Register events
            _levelLoadBinding = new EventBinding<LevelLoadEvent>(OnLevelLoadRequested);
            EventBus<LevelLoadEvent>.Register(_levelLoadBinding);


            var bombDiffusedEventBinding = new EventBinding<BombDefusedEvent>(BombDefused);
            var enemyKilledEventBinding = new EventBinding<EnemyKilledEvent>(EnemyKilled);
            var hostageRescuedEventBinding = new EventBinding<HostageRescuedEvent>(HostageRescued);
            var levelLostEventBinding = new EventBinding<PlayerKilledEvent>(LevelFailed);
            
            EventBus<PlayerKilledEvent>.Register(levelLostEventBinding);
            EventBus<BombDefusedEvent>.Register(bombDiffusedEventBinding);
            EventBus<EnemyKilledEvent>.Register(enemyKilledEventBinding);
            EventBus<HostageRescuedEvent>.Register(hostageRescuedEventBinding);
           
         
            
        }


        private void OnDestroy()
        {
            EventBus<LevelLoadEvent>.Deregister(_levelLoadBinding);
        }


        private void BombDefused()
        {
            if (!IsLevelActive) return;
            _remainingBombs--;
            if (LevelWon) LevelCompleted();
        }

        private void HostageRescued()
        {
            if (!IsLevelActive) return;
            _remainingHostages--;
            if (LevelWon) LevelCompleted();
        }


        public void LoadLevel(LevelConfig config)
        {
            if (config == null)
            {
                Debug.LogError("Attempted to load null level config!");
                return;
            }

            StartCoroutine(LoadLevelCoroutine(config));
        }

        private IEnumerator LoadLevelCoroutine(LevelConfig config)
        {
          

            // Cleanup previous level
            CleanupCurrentLevel();

            // Load the scene
            var asyncLoad = SceneManager.LoadSceneAsync(config.levelName);

            while (asyncLoad is { isDone: false })
            {
                IsLoading = true;
                EventBus<LevelLoadProgressEvent>.Raise(new LevelLoadProgressEvent(asyncLoad.progress));
                yield return null;
            }

            // Initialize new level
            _currentLevel = config;
            InitializeLevel(config);

            IsLoading = false;
            
        }

        private void InitializeLevel(LevelConfig config)
        {
            _remainingEnemies = config.enemyCount;
            IsLevelActive = true;

            // Setup mission strategy based on level type


            EventBus<LevelStartedEvent>.Raise(new LevelStartedEvent(config));
        }


        private void EnemyKilled()
        {
            if (!IsLevelActive) return;

            _remainingEnemies--;

            if (LevelWon) LevelCompleted();
        }

        private void LevelCompleted()
        {
            IsLevelActive = false;
            EventBus<LevelWonEvent>.Raise(new LevelWonEvent());
        }

        public void LevelFailed()
        {
            IsLevelActive = false;
            EventBus<LevelLostEvent>.Raise(new());
        }

        public void ReloadLevel()
        {
            if (_currentLevel != null) LoadLevel(_currentLevel);
        }


        public void UnloadCurrentLevel()
        {
            CleanupCurrentLevel();
            _currentLevel = null;
        }

        private void CleanupCurrentLevel()
        {
            IsLevelActive = false;
        }

        private void OnLevelLoadRequested(LevelLoadEvent evt)
        {
            LoadLevel(evt.LevelConfig);
        }
    }

    // ========================
    // EVENT DEFINITIONS
    // ========================

    public struct LevelLoadProgressEvent : IEvent
    {
        public float Progress;

        public LevelLoadProgressEvent(float progress)
        {
            Progress = progress;
        }
    }

    public struct LevelStartedEvent : IEvent
    {
        public LevelConfig Level;

        public LevelStartedEvent(LevelConfig level)
        {
            Level = level;
        }
    }
}

// ========================
// ENEMY INTEGRATION EXAMPLE
// ========================