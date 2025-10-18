using System;
using General;
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
            get => string.IsNullOrEmpty(strategyTypeName.ToString())
                ? null
                : Type.GetType(typeof(IMissionStrategy).Namespace + '.' + strategyTypeName);
            set
            {
                strategyTypeName = MissionType.KillAllEnemiesStrategy;
                Enum.TryParse(value?.AssemblyQualifiedName, out strategyTypeName);
            }
        }

        public IMissionStrategy CreateInstance()
        {
            VandullLogger.Log(strategyTypeName.ToString());
            if (StrategyType == null) return null;
            return (IMissionStrategy)Activator.CreateInstance(StrategyType);
        }
    }
}