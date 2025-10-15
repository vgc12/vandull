using EventBus;
using Npcs;
using Player;
using UnityEngine;

namespace Levels
{
    public class KillAllEnemiesStrategy : IMissionStrategy
    {
        private int _remainingEnemies;
        private bool _playerDead;
        public KillAllEnemiesStrategy()
        {
            var enemyKilledEventBinding = new EventBinding<EnemyKilledEvent>(OnEnemyKilled);
            var playerDeathEventBinding = new EventBinding<PlayerDeathEvent>(OnPlayerKilled);
            
            EventBus<EnemyKilledEvent>.Register(enemyKilledEventBinding);
            EventBus<PlayerDeathEvent>.Register(playerDeathEventBinding);
            
            var totalEnemies = Object.FindObjectsByType<Enemy>(findObjectsInactive: FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
            _remainingEnemies = totalEnemies;
        }

        private void OnPlayerKilled(PlayerDeathEvent obj)
        {
            _playerDead = true; 
        }

        private void OnEnemyKilled(EnemyKilledEvent obj)
        {
            _remainingEnemies--;
        }

        public bool IsMissionComplete()
        {
            return _remainingEnemies <= 0;
        }

        public bool IsMissionFailed() => _playerDead;
  
    }
}