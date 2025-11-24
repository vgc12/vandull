using System.Collections.Generic;
using DependencyInjection;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace Levels
{
    [CreateAssetMenu(fileName = "New Level", menuName = "Level/Level")]
    public class Level : ScriptableObject
    {
        [SerializeField] private string levelName;
        [SerializeField] private string sceneName;
        [TextArea] public string description;

        [SerializeField] private Objective requiredObjective;
        [SerializeField] private List<Objective> optionalObjectives = new();

        private bool _levelCompleted;

        private ILogger _logger;

        public string LevelName => levelName;
        public string GetSceneName => sceneName;
        public string GetDescription => description;
        public Objective GetRequiredObjective => requiredObjective;
        public List<Objective> GetOptionalObjectives => optionalObjectives;

        private void OnEnable()
        {
            RuntimeResolver.Instance.TryResolve(out _logger);
        }

        public void InitializeLevel()
        {
            _levelCompleted = false;

            if (requiredObjective != null)
                requiredObjective.Initialize();

            foreach (var objective in optionalObjectives)
                if (objective != null)
                    objective.Initialize();
        }

        public void CheckLevelCompletion()
        {
            if (!requiredObjective)
            {
                _logger.LogError("Level has no required objective!");
                return;
            }

            requiredObjective.CheckCompletion();

            foreach (var objective in optionalObjectives)
                if (objective != null)
                    objective.CheckCompletion();

            // Level is complete if required objective is completed
            _levelCompleted = requiredObjective.IsCompleted;
        }

        public bool IsLevelCompleted()
        {
            return _levelCompleted;
        }

        public float GetCompletionPercentage()
        {
            var totalObjectives = 1 + optionalObjectives.Count; // Required + optional
            var completedObjectives = requiredObjective.IsCompleted ? 1 : 0;

            foreach (var objective in optionalObjectives)
                if (objective != null && objective.IsCompleted)
                    completedObjectives++;

            return (float)completedObjectives / totalObjectives;
        }

        public void ResetLevel()
        {
            _levelCompleted = false;

            if (requiredObjective != null)
                requiredObjective.Reset();

            foreach (var objective in optionalObjectives)
                if (objective != null)
                    objective.Reset();
        }
    }
}