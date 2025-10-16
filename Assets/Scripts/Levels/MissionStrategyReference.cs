using System;
using Levels.Strategies;
using UnityEngine;

namespace Levels
{
    [Serializable]
    public class MissionStrategyReference
    {
        [SerializeField] private MissionType strategyTypeName;

        public Type StrategyType
        {
            get => string.IsNullOrEmpty(strategyTypeName.ToString()) ? null : Type.GetType(strategyTypeName.ToString());
            set
            {
                strategyTypeName = MissionType.KillAllEnemiesStrategy;
                Enum.TryParse(value?.AssemblyQualifiedName, out strategyTypeName);
            }
        }

        public IMissionStrategy CreateInstance()
        {
            if (StrategyType == null) return null;
            return (IMissionStrategy)Activator.CreateInstance(StrategyType);
        }
    }
}