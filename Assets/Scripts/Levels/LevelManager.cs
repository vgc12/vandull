using System.Collections;
using System.Collections.Generic;
using DependencyInjection;
using EventBus;
using Levels.Strategies;
using Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;
using ILogger = General.Logging.ILogger;


namespace Levels
{
    public sealed class LevelManager : PersistentSingleton<LevelManager>
    {
        [SerializeField] private Level currentLevel;

        public List<Level> levels;

        private EventBinding<EnemyKilledEvent> _enemyKilledBinding;

        private EventBinding<LevelLoadEvent> _levelLoadEventBinding;

        private ILogger _logger;
        private EventBinding<PlayerKilledEvent> _playerKilledBinding;

        private EventBinding<TargetEnemyKilledEvent> _targetEnemyKilledBinding;

        public bool IsLoading { get; private set; }
        public bool IsLevelActive { get; private set; }

        public bool IsCompleted { get; private set; }


        public Level GetCurrentLevel
        {
            get => currentLevel;
            set
            {
                currentLevel = value;
                currentLevel.InitializeLevel();
            }
        }

        private void Start()
        {
            _enemyKilledBinding = new EventBinding<EnemyKilledEvent>(OnEnemyKilled);
            _targetEnemyKilledBinding = new EventBinding<TargetEnemyKilledEvent>(OnTargetKilled);
            _levelLoadEventBinding = new EventBinding<LevelLoadEvent>(OnLevelLoadEvent);
            _playerKilledBinding = new EventBinding<PlayerKilledEvent>(OnPlayerKilled);

            EventBus<LevelLoadEvent>.Register(_levelLoadEventBinding);
            EventBus<TargetEnemyKilledEvent>.Register(_targetEnemyKilledBinding);
            EventBus<PlayerKilledEvent>.Register(_playerKilledBinding);
            EventBus<EnemyKilledEvent>.Register(_enemyKilledBinding);
        }

        private void OnEnable()
        {
            RuntimeResolver.Instance.TryResolve(out _logger);
        }

        private void OnPlayerKilled(PlayerKilledEvent obj)
        {
            OnLevelFailed();
        }

        private void OnLevelLoadEvent(LevelLoadEvent e)
        {
            LoadLevel(e.LevelConfig);
        }

        private void OnTargetKilled(TargetEnemyKilledEvent obj)
        {
            CheckLevelComplete();
        }

        private void OnEnemyKilled(EnemyKilledEvent obj)
        {
            CheckLevelComplete();
        }

        private void CheckLevelComplete()
        {
            if (!IsLevelActive) return;

            currentLevel.CheckLevelCompletion();

            if (currentLevel.IsLevelCompleted()) OnLevelComplete();
        }


        private void OnLevelComplete()
        {
            IsLevelActive = false;
            IsCompleted = true;
            _logger.Log($"Level '{currentLevel.LevelName}' completed!");
            EventBus<LevelWonEvent>.Raise(new LevelWonEvent());
        }

        private void OnLevelFailed()
        {
            IsLevelActive = false;
            _logger.Log($"Level '{currentLevel.LevelName}' failed!");
            EventBus<LevelLostEvent>.Raise(new LevelLostEvent());
        }

        public void LoadLevel(Level config)
        {
            if (config == null)
            {
                _logger.LogError("Attempted to load null level config!");
                return;
            }

            StartCoroutine(LoadLevelCoroutine(config));
        }

        private IEnumerator LoadLevelCoroutine(Level config)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(config.GetSceneName);

            while (asyncLoad is { isDone: false })
            {
                IsLoading = true;
                EventBus<LevelLoadProgressEvent>.Raise(new LevelLoadProgressEvent(asyncLoad.progress));
                yield return null;
            }

            // Initialize new level
            currentLevel = config;

            if (currentLevel != null)
                currentLevel.InitializeLevel();
            IsLevelActive = true;
            IsLoading = false;
            EventBus<LevelStartedEvent>.Raise(new LevelStartedEvent(config));
        }


        public void ReloadLevel()
        {
            if (currentLevel != null) LoadLevel(currentLevel);
        }
    }
}