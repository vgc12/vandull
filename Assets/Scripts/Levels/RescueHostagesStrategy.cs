using Levels.Strategies;

namespace Levels
{
    [MissionType("Rescue Operation", "Extract all hostages safely")]
    public class RescueHostagesStrategy : IMissionStrategy
    {
        public string MissionName { get; }

        public void Initialize(LevelConfig config)
        {
        }

        public void OnEnemyKilled()
        {
        }

        public void OnHostageRescued()
        {
        }

        public void OnBombDefused()
        {
        }

        public void Reset()
        {
        }

        public void Cleanup()
        {
        }
    }
}