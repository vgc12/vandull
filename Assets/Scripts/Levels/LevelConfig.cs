using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        public string levelName;
        public LevelType levelType = LevelType.TargetAssassination;
        public int enemyCount = 5;
        public string levelDescription;
        public int difficultyLevel = 1;
    }

    public enum LevelType
    {
        TargetAssassination,
        HostageRescue,
        IntelSecuring
    }
}