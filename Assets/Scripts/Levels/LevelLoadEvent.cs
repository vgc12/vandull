using EventBus;

namespace Levels
{
    public class LevelLoadEvent : IEvent
    {
        public readonly LevelConfig LevelConfig;

        public LevelLoadEvent(LevelConfig levelConfig)
        {
            LevelConfig = levelConfig;
        }
    }
}