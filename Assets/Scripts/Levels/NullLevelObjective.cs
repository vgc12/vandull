using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(fileName = "Null Level Objective", menuName = "Level/Objectives/Null Objective", order = 1)]
    public class NullLevelObjective : Objective
    {
        public override void Reset()
        {
            IsCompleted = false;
        }

        public override void Initialize()
        {
            IsCompleted = false;
        }

        public override void CheckCompletion()
        {
            // Null objective never completes
        }
    }
}