namespace Levels.Strategies
{
    public interface IMissionStrategy
    {
        string MissionName { get; }
        void Initialize(LevelConfig config);
        void OnEnemyKilled();
        void OnHostageRescued();
        void OnBombDefused();
        void Reset();
        void Cleanup();
    }
}