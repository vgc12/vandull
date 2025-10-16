using EventBus;

namespace Levels
{
    public class LevelEvent : IEvent
    {
        public LevelEventType EventType { get; }
        public object Data { get; }
    
        public LevelEvent(LevelEventType eventType, object data = null)
        {
            
            EventType = eventType;
            Data = data;
        }
    }
}