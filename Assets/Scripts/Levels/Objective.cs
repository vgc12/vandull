using DependencyInjection;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace Levels
{
    public abstract class Objective : ScriptableObject
    {
        [TextArea] public string description;
        public bool isRequired = true;

        protected ILogger Logger;
        public bool IsCompleted { get; protected set; }

        private void Awake()
        {
            RuntimeResolver.Instance.TryResolve(out Logger);
        }

        public abstract void Reset();

        public abstract void Initialize();
        public abstract void CheckCompletion();
    }
}