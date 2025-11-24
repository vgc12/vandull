using EventBus;

public struct LevelLoadProgressEvent : IEvent
{
    public float Progress;

    public LevelLoadProgressEvent(float progress)
    {
        Progress = progress;
    }
}