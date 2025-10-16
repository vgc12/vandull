using EventBus;
using General.Game;
using Levels.Strategies;
using Npcs;
using Player;

namespace Levels
{
    [MissionType("Eliminate All Hostiles", "Neutralize all enemy combatants")]
    public class KillAllEnemiesStrategy : IMissionStrategy
    {
        private LevelConfig _config;

        private EventBinding<EnemyKilledEvent> _enemyKilledEventBinding;
        private bool _playerDead;
        private EventBinding<PlayerDeathEvent> _playerDeathEventBinding;
        private int _remainingEnemies;


        public string MissionName { get; private set; }

        public void Initialize(LevelConfig config)
        {
            _enemyKilledEventBinding = new EventBinding<EnemyKilledEvent>(OnEnemyKilled);
            _playerDeathEventBinding = new EventBinding<PlayerDeathEvent>(OnPlayerKilled);

            EventBus<EnemyKilledEvent>.Register(_enemyKilledEventBinding);
            EventBus<PlayerDeathEvent>.Register(_playerDeathEventBinding);
            _config = config;
            Reset();
        }

        public void OnEnemyKilled()
        {
            _remainingEnemies--;

            if (_remainingEnemies <= 0)
                EventBus<GameStateChangedEvent>.Raise(new GameStateChangedEvent(GameState.MissionComplete));
        }

        public void OnHostageRescued()
        {
        }

        public void OnBombDefused()
        {
        }

        public void Reset()
        {
            if (!_config) return;
            var totalEnemies = _config.enemyCount;
            _remainingEnemies = totalEnemies;

            MissionName = _config.name;
        }

        public void Cleanup()
        {
            EventBus<EnemyKilledEvent>.Deregister(_enemyKilledEventBinding);
            EventBus<PlayerDeathEvent>.Deregister(_playerDeathEventBinding);
        }


        public void OnPlayerKilled()
        {
            EventBus<GameStateChangedEvent>.Raise(new GameStateChangedEvent(GameState.MissionFailed));
        }
    }
}