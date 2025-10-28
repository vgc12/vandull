using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Levels
{
    public interface ISpawner<T> where T : Component
    {
        public UniTask<IEnumerable<T>> SpawnAsync(int amount = 1, CancellationToken ct = default);
    }
}