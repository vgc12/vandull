using EventBus;

namespace Levels
{
    public sealed class LevelLoadEvent : IEvent
    {
        public readonly Level LevelConfig;

        public LevelLoadEvent(Level levelConfig)
        {
            LevelConfig = levelConfig;
        }
    }
}