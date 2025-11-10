using Npcs;
using UnityEngine;

namespace Levels
{
    public class EnemySpawner : PrefabSpawner<Enemy>
    {
        public EnemySpawner(GameObject prefab) : base(prefab)
        {
        }
    }

    public class AudioSpawner : ComponentSpawner<AudioSource>
    {
    }
}