using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(fileName = "New Rescue Hostage Objective", menuName = "Level/Objectives/Rescue Hostage")]
    public class RescueHostageObjective : Objective
    {
        [SerializeField] private int hostagesToRescue = 1;
        private int _hostagesRescued;

        public override void Reset()
        {
            IsCompleted = false;
            _hostagesRescued = 0;
        }

        public override void Initialize()
        {
            IsCompleted = false;
            _hostagesRescued = 0;
        }

        public override void CheckCompletion()
        {
            IsCompleted = _hostagesRescued >= hostagesToRescue;
        }

        public void RescueHostage()
        {
            _hostagesRescued++;
            CheckCompletion();
        }

        public int GetProgress()
        {
            return _hostagesRescued;
        }
    }
}