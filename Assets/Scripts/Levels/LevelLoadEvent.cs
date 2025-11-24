using EventBus;

namespace Levels
{
    public class LevelLoadEvent : IEvent
    {
        public readonly Level LevelConfig;

        public LevelLoadEvent(Level levelConfig)
        {
            LevelConfig = levelConfig;
        }
    }
}