using EventBus;
using Levels;

public struct LevelStartedEvent : IEvent
{
    public Level Level;

    public LevelStartedEvent(Level level)
    {
        Level = level;
    }
}