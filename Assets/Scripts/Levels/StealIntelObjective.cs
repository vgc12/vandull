using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(fileName = "New Steal Intel Objective", menuName = "Level/Objectives/Steal Intel")]
    public class StealIntelObjective : Objective
    {
        [SerializeField] private int intelItemsRequired = 1;

        public int GetProgress { get; private set; }

        public override void Reset()
        {
            IsCompleted = false;
            GetProgress = 0;
        }

        public override void Initialize()
        {
            IsCompleted = false;
            GetProgress = 0;
        }

        public override void CheckCompletion()
        {
            IsCompleted = GetProgress >= intelItemsRequired;
        }

        public void CollectIntel()
        {
            GetProgress++;
            CheckCompletion();
        }
    }
}