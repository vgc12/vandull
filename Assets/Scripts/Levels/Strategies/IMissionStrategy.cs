namespace Levels.Strategies
{
    public interface IMissionStrategy
    {
        string MissionName { get; }
        bool Initialized { get; }
        void Initialize(LevelConfig config);
        void OnEnemyKilled();
        void OnHostageRescued();
        void OnBombDefused();
        void Reset();
        void Cleanup();
    }
}