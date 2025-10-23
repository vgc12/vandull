using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        public string levelName;

        public int enemyCount = 5;
        public int hostageCount = 2;
        public int bombCount = 3;
        public string levelDescription;
        public int difficultyLevel = 1;
    }
}