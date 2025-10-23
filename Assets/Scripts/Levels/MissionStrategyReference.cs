using System;
using Levels.Strategies;
using Reflex.Attributes;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace Levels
{
    [Serializable]
    public class MissionStrategyReference
    {
        [SerializeField] private MissionType strategyTypeName;

        [Inject] private readonly ILogger _logger;

        public Type StrategyType
        {
            get => string.IsNullOrEmpty(strategyTypeName.ToString())
                ? null
                : Type.GetType(typeof(IMissionStrategy).Namespace + '.' + strategyTypeName);
            set
            {
                strategyTypeName = MissionType.Elimination;
                Enum.TryParse(value?.AssemblyQualifiedName, out strategyTypeName);
            }
        }

        public IMissionStrategy CreateInstance()
        {
            _logger.Log(strategyTypeName.ToString());
            if (StrategyType == null) return null;
            return (IMissionStrategy)Activator.CreateInstance(StrategyType);
        }
    }
}