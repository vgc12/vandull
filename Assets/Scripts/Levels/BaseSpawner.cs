using System;
using System.Collections.Generic;
using System.Threading;
using Attributes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Levels
{
    public abstract class BaseSpawner<T> : ISpawner<T> where T : Component
    {
        [Required] public GameObject prefab;

        public async UniTask<IEnumerable<T>> SpawnAsync(int amount = 1, CancellationToken ct = default)
        {
            var objects = await Object.InstantiateAsync(prefab, amount);
            var components = new List<T>(objects.Length);
            if (components == null) throw new ArgumentNullException(nameof(components));

            foreach (var obj in objects)
            {
                obj.TryGetComponent<T>(out var component);
                if (component) components.Add(component);
            }
            // All spawn in parallel, wait for all to complete

            return components;
        }
    }
}