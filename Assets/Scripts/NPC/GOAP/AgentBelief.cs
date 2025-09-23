using System;
using System.Collections.ObjectModel;
using UnityEngine;


public class AgentBelief
{
    public string Name { get; }
    private Func<bool> _condition;
    private Func<Vector3> _observedLocation = () => Vector3.zero;

    public Vector3 Location => _observedLocation();

    public AgentBelief(string name) => Name = name;

    public bool Evaluate() => _condition();

    public class Builder
    {
        private readonly AgentBelief _belief;

        public Builder(string name)
        {
            _belief = new AgentBelief(name);
        }

        public Builder WithCondition(Func<bool> condition)
        {
            _belief._condition = condition;
            return this;
        }

        public Builder WithLocation(Func<Vector3> observedLocation)
        {
            _belief._observedLocation = observedLocation;
            return this;
        }

        public AgentBelief Build()
        {
            if (_belief._condition == null)
                throw new InvalidOperationException("Condition must be set before building the belief.");
            return _belief;
        }
    }
}