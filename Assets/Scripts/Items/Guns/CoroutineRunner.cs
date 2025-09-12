using System.Collections;
using UnityEngine;

namespace Items.Guns
{
    public static class CoroutineRunner
    {
        private static MonoBehaviour _runner;

        public static void Initialize(MonoBehaviour runner)
        {
            _runner = runner;
        }

        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return _runner?.StartCoroutine(routine);
        }

        public static void StopCoroutine(Coroutine routine)
        {
            if (routine != null)
                _runner?.StopCoroutine(routine);
        }
    }
}