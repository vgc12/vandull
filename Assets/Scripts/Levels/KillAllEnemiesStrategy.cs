using EventBus;

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

        private int _remainingEnemies;


        public string MissionName { get; private set; }

        public void Initialize(LevelConfig config)
        {
            _enemyKilledEventBinding = new EventBinding<EnemyKilledEvent>(OnEnemyKilled);


            EventBus<EnemyKilledEvent>.Register(_enemyKilledEventBinding);
            _config = config;
            Reset();
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
   
        }


        public void OnPlayerKilled()
        {
            EventBus<LevelEvent>.Raise(new LevelEvent(LevelEventType.LevelLost));   
        }
    }

    public enum LevelEventType
    {
        EnemyKilled,
        HostageRescued,
        BombDefused,
        ObjectiveCompleted,
        LevelWon,
        LevelLost
    }
}