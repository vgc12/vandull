namespace StateMachine
{
    public sealed class Transition : ITransition
    {
        public Transition(IState to, IPredicate predicate)
        {
            Predicate = predicate;
            To = to;
        }

        public IPredicate Predicate { get; }
        public IState To { get; }
    }
}