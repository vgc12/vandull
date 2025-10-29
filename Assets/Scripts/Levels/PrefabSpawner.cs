using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using General.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Levels
{
    public abstract class ComponentSpawner<T> : ISpawner<T> where T : Component
    {
        public IEnumerable<T> Spawn(int amount = 1, Action<GameObject> onSpawn = null)
        {
            var components = new List<T>(amount);
            for (var i = 0; i < amount; i++)
            {
                var go = new GameObject(typeof(T).Name);
                var component = go.GetOrAdd<T>();

                onSpawn?.Invoke(go);

                components.Add(component);
            }

            return components;
        }


        public async UniTask<IEnumerable<T>> SpawnAsync(int amount = 1, Action<GameObject> onSpawn = null,
            CancellationToken ct = default)
        {
            // Create tasks for all spawns
            var spawnTasks = new List<UniTask<T>>(amount);

            for (var i = 0; i < amount; i++) spawnTasks.Add(SpawnSingleAsync<T>(onSpawn, ct));

            // Wait for all spawns to complete in parallel
            var components = await UniTask.WhenAll(spawnTasks);

            return components;
        }

        /// <summary>
        ///     Helper method to spawn a single component asynchronously.
        /// </summary>
        private async UniTask<T> SpawnSingleAsync<T>(Action<GameObject> onSpawn, CancellationToken ct)
            where T : Component
        {
            ct.ThrowIfCancellationRequested();

            // Yield to allow parallel execution
            await UniTask.Yield(PlayerLoopTiming.Update, ct);

            var go = new GameObject(typeof(T).Name);
            var component = go.GetOrAdd<T>();

            onSpawn?.Invoke(go);

            return component;
        }
    }

    public abstract class PrefabSpawner<T> : ISpawner<T> where T : Component
    {
        protected PrefabSpawner(GameObject prefab)
        {
            Prefab = prefab;
        }

        private GameObject Prefab { get; }

        public IEnumerable<T> Spawn(int amount = 1, Action<GameObject> onSpawn = null)
        {
            var components = new List<T>(amount);
            for (var i = 0; i < amount; i++)
            {
                var obj = Object.Instantiate(Prefab);
                obj.TryGetComponent<T>(out var component);
                if (component) components.Add(component);
                onSpawn?.Invoke(obj.gameObject);
            }

            return components;
        }

        public async UniTask<IEnumerable<T>> SpawnAsync(int amount = 1, Action<GameObject> onSpawn = null,
            CancellationToken ct = default)
        {
            var objects = await Object.InstantiateAsync(Prefab, amount);
            var components = new List<T>(objects.Length);
            if (components == null) throw new ArgumentNullException(nameof(components));

            foreach (var obj in objects)
            {
                obj.TryGetComponent<T>(out var component);
                if (component) components.Add(component);
                onSpawn?.Invoke(obj);
            }

            return components;
        }
    }
}