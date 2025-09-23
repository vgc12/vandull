using System.Collections.Generic;

namespace NPC.GOAP
{
    public class AgentGoal
    {
        public string Name { get; }
        public float Priority { get; private set; } = 1f;
        public HashSet<AgentBelief> DesiredEffects { get; } = new();
    
        private AgentGoal(string name) => Name = name;

        public class Builder
        {
            private readonly AgentGoal _goal;

            public Builder(string name)
            {
                _goal = new AgentGoal(name);
            }

            public Builder WithDesiredEffect(AgentBelief belief)
            {
                _goal.DesiredEffects.Add(belief);
                return this;
            }
        
            public Builder WithPriority(float priority)
            {
                _goal.Priority = priority;
                return this;
            }

            public AgentGoal Build()
            {
                return _goal;
            }
        }
    }
}