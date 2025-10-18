using EventBus;
using Levels.Strategies;

namespace Levels
{
    public class LevelEvent : IEvent
    {
        public LevelEvent(LevelEventType eventType, object data = null)
        {
            EventType = eventType;
            Data = data;
        }

        public LevelEventType EventType { get; }
        public object Data { get; }
    }
}