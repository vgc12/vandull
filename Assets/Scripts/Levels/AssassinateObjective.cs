using System.Collections.Generic;
using System.Linq;
using EventBus;
using Levels.Strategies;
using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(fileName = "New Assassinate Objective", menuName = "Level/Objectives/Assassinate")]
    public class AssassinateObjective : Objective
    {
        private List<AssassinationTarget> _aliveTargets;
        private List<AssassinationTarget> _targetEntities;
        private EventBinding<TargetEnemyKilledEvent> _targetKilledBinding;

        public override void Reset()
        {
            EventBus<TargetEnemyKilledEvent>.Deregister(_targetKilledBinding);
            IsCompleted = false;
            Initialize();
        }

        public override void Initialize()
        {
            IsCompleted = false;
            _targetKilledBinding = new EventBinding<TargetEnemyKilledEvent>(OnTargetKilled);
            EventBus<TargetEnemyKilledEvent>.Register(_targetKilledBinding);

            _targetEntities = FindObjectsByType<AssassinationTarget>(FindObjectsSortMode.None).ToList();
            _aliveTargets = new List<AssassinationTarget>(_targetEntities);


            if (_targetEntities.Count == 0) Logger.LogWarning("No targets found in scene");
        }

        private void OnTargetKilled(TargetEnemyKilledEvent obj)
        {
            if (_aliveTargets.Contains(obj.Enemy)) _aliveTargets.Remove(obj.Enemy);
        }

        public override void CheckCompletion()
        {
            if (_aliveTargets.Count == 0) IsCompleted = true;
        }
    }
}