using System.Collections.Generic;
using System.Linq;
using General;
using NPC.GOAP;
using UnityEngine;

public interface IGoapPlanner
{
    ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);
}

public class GoapPlanner : IGoapPlanner
{
    public ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null)
    {
        // Order goals by priority, descending
        var orderedGoals = goals
            .Where(g => g.DesiredEffects.Any(b => !b.Evaluate()))
            .OrderByDescending(g => g == mostRecentGoal ? g.Priority - 0.01 : g.Priority)
            .ToList();

        foreach (var goal in orderedGoals)
        {
            Node goalNode = new Node(null, null, goal.DesiredEffects, 0);
            
            // if path is found, return the plan
            if (FindPath(goalNode, agent.Actions))
            {
                if(goalNode.IsLeafDead) continue;
                Stack<AgentAction> actions = new Stack<AgentAction>();
                while (goalNode.Leaves.Count > 0)
                {
                    var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                    goalNode = cheapestLeaf;
                    actions.Push(cheapestLeaf.Action);
                }
                
                return new ActionPlan(goal, actions, goalNode.Cost);
            }
        }
//        VandullLogger.LogWarning("No Plan Found");
        return null;
        
    }

    private bool FindPath(Node parent, HashSet<AgentAction> actions)
    {
        foreach (var action in actions)
        {
            var requiredEffects = parent.RequiredEffects;
            
            // Remove any true beliefs there is no action to take
            requiredEffects.RemoveWhere(b => b.Evaluate());
            
            // If there are no required effects to fufill we have a plan
            if (requiredEffects.Count == 0)
            {
                return true;
            }

            if (action.Effects.Any(requiredEffects.Contains))
            {
                var newRequiredEffects = new HashSet<AgentBelief>(requiredEffects);
                newRequiredEffects.ExceptWith(action.Effects);
                newRequiredEffects.UnionWith(action.Preconditions);

                var newAvailableActions = new HashSet<AgentAction>(actions);
                newAvailableActions.Remove(action);
                
                var newNode = new Node(parent, action,  newRequiredEffects, parent.Cost + action.Cost);
                
                // Explore the new node recursively
                if (FindPath(newNode, newAvailableActions))
                {
                    parent.Leaves.Add(newNode);
                    newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
                    
                }
                
                // If all effects at this depth have been satisfied, return true
                if (newRequiredEffects.Count == 0)
                {
                    return true;
                }
                
            }
        }
        return false;
    }
}

public class Node
{
    public Node Parent { get; }
    public AgentAction Action { get;  }
    public HashSet<AgentBelief> RequiredEffects { get; }
    public List<Node> Leaves { get; }
    public float Cost { get; }

    public bool IsLeafDead => Leaves.Count == 0 && Action == null;

    public Node(Node parent, AgentAction action, HashSet<AgentBelief> effects, float cost)
    {
        Parent = parent;
        Action = action;
        Cost = cost;
        RequiredEffects = new HashSet<AgentBelief>(effects);
        Leaves = new List<Node>();
    }


}

public class ActionPlan
{
    public AgentGoal AgentGoal { get; }
    public Stack<AgentAction> Actions { get; }
    public float TotalCost { get; }

    public ActionPlan(AgentGoal agentGoal, Stack<AgentAction> actions, float totalCost)
    {
        AgentGoal = agentGoal;
        Actions = actions;
        TotalCost = totalCost;
    }
    
}


public class AgentAction
{
    public string Name { get; }
    public float Cost { get; private set; }

    public HashSet<AgentBelief> Preconditions { get; } = new();
    public HashSet<AgentBelief> Effects { get; } = new();

    public IActionStrategy Strategy => _strategy;
    
    private IActionStrategy _strategy;

    public bool Complete => _strategy.Complete;
    
    public void Start() => _strategy.Start();

    private AgentAction(string name)
    {
        Name = name;
    }

    public class Builder
    {
        private readonly AgentAction _action;

        public Builder(string name)
        {
            _action = new AgentAction(name)
            {
                Cost = 1f
            };
        }
        
        public Builder WithCost(float cost)
        {
            _action.Cost = cost;
            return this;
        }
        
        public Builder AddPrecondition(AgentBelief belief)
        {
            _action.Preconditions.Add(belief);
            return this;
        }
        public Builder AddEffect(AgentBelief belief)
        {
            _action.Effects.Add(belief);
            return this;
        }
        public Builder WithStrategy(IActionStrategy strategy)
        {
            _action._strategy = strategy;
            return this;
        }
        
        public AgentAction Build()
        {
            return _action;
        }
    }
    
    public void Update(float deltaTime)
    {
        // check if strategy can be performed and update
        if (_strategy.CanPerform)
        {
            _strategy.Update(deltaTime);
        }
        
        // return if strategy is not complete
        if(!_strategy.Complete) return;
        
        // Apply effects
        foreach (var effect in Effects)
        {
            effect.Evaluate();
        }
    }

    public void Stop() => _strategy.Stop();

}