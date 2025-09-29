using System;
using System.Collections.Generic;
using UnityEngine;

namespace NPC.GOAP
{
    public class BeliefFactory
    {
        private readonly GoapAgent _agent;
        private readonly Dictionary<BeliefType, AgentBelief> _beliefs;

        public BeliefFactory(GoapAgent agent, Dictionary<BeliefType, AgentBelief> beliefs)
        {
            _agent = agent;
            _beliefs = beliefs;
        }

        public void AddBelief(BeliefType key, Func<bool> condition)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key.ToString())
                .WithCondition(condition)
                .Build());
        }

        public void AddLocationBelief(BeliefType key, float distance, Transform locationCondition)
        {
            AddLocationBelief(key,distance,locationCondition.position);
        }

        public void AddSensorBelief(BeliefType key, ISensor sensor) 
        {
            _beliefs.Add(key, new AgentBelief.Builder(key.ToString())
                .WithCondition(() => sensor.CanSeeTarget)
                .WithLocation(() => sensor.Target.position)
                .Build());
        }
    
        public void AddLocationBelief(BeliefType key, float distance, Vector3 locationCondition)
        {
            _beliefs.Add(key, new AgentBelief.Builder(key.ToString())
                .WithCondition(() => InRangeOf(locationCondition, distance))
                .WithLocation(() => locationCondition)
                .Build());
        }

        private bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(_agent.transform.position, pos) < range;
    }

    public interface ISensor
    {
        public bool CanSeeTarget { get; }
        public Transform Target { get; }
    }
}