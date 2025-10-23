using System;
using System.Collections.Generic;

namespace StateMachine
{
    public class StateMachine
    {
        private readonly HashSet<ITransition> _anyTransitions = new();

        private readonly Dictionary<Type, StateNode> _nodes = new();
        private StateNode _currentState;
        public IState CurrentState => _currentState.State;

        public void Update()
        {
            var transition = GetTransition();
            if (transition != null) ChangeState(transition.To);
            _currentState.State?.Update();
        }

        public void ChangeState(IState to)
        {
            if (_currentState != null && _currentState.State == to)
                return;
            var previousState = _currentState?.State;
            var nextState = _nodes[to.GetType()].State;
            previousState?.Exit();
            nextState?.Enter();
            _currentState = _nodes[to.GetType()];
        }

        public void ChangeState(Type to)
        {
            if (_currentState != null && _currentState.State.GetType() == to)
                return;
            var previousState = _currentState?.State;
            var nextState = _nodes[to].State;
            previousState?.Exit();
            nextState?.Enter();
            _currentState = _nodes[to];
        }


        private ITransition GetTransition()
        {
            foreach (var transition in _anyTransitions)
                if (transition.Predicate.Evaluate())
                    return transition;

            foreach (var transition in _currentState.Transitions)
                if (transition.Predicate.Evaluate())
                    return transition;

            return null;
        }

        public void FixedUpdate()
        {
            _currentState.State?.FixedUpdate();
        }

        public void SetState(IState state)
        {
            _currentState = _nodes[state.GetType()];
        }

        public void AddState(IState state)
        {
            GetOrAddNode(state);
        }

        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            AddTransition(from, to, new FuncPredicate(predicate));
        }

        public void AddAnyTransition(IState to, Func<bool> predicate)
        {
            AddAnyTransition(to, new FuncPredicate(predicate));
        }

        public void AddAnyTransition(IState to, IPredicate condition)
        {
            _anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
        }

        private StateNode GetOrAddNode(IState state)
        {
            var node = _nodes.GetValueOrDefault(state.GetType());
            if (node == null)
            {
                node = new StateNode(state);
                _nodes.Add(state.GetType(), node);
            }

            return node;
        }


        private class StateNode
        {
            public StateNode(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            public IState State { get; }
            public HashSet<ITransition> Transitions { get; }

            public void AddTransition(IState to, IPredicate predicate)
            {
                Transitions.Add(new Transition(to, predicate));
            }
        }
    }
}