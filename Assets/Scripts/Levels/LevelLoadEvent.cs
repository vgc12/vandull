using EventBus;

namespace Levels
{
    public abstract class LevelLoadEvent : IEvent
    {
        public readonly LevelConfig LevelConfig;

        public LevelLoadEvent(LevelConfig levelConfig)
        {
            LevelConfig = levelConfig;
        }
    }
}