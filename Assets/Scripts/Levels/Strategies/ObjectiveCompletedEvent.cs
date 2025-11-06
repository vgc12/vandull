using EventBus;

namespace Levels.Strategies
{
    public struct ObjectiveCompletedEvent : IEvent
    {
        public string ObjectiveId;
        public string ObjectiveName;

        public ObjectiveCompletedEvent(string objectiveId, string objectiveName)
        {
            ObjectiveId = objectiveId;
            ObjectiveName = objectiveName;
        }
    }
}