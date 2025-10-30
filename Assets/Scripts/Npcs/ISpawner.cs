using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Levels
{
    public interface ISpawner<T> where T : Component
    {
        public IEnumerable<T> Spawn(int amount = 1, Action<GameObject> onSpawn = null);

        public UniTask<IEnumerable<T>> SpawnAsync(int amount = 1, Action<GameObject> onSpawn = null,
            CancellationToken ct = default);
    }
}