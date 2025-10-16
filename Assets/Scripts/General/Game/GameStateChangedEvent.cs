using EventBus;

namespace General.Game
{
    public class GameStateChangedEvent : IEvent
    {
        public GameStateChangedEvent(GameState newGameState)
        {
            NewGameState = newGameState;
        }

        public GameState NewGameState { get; }
    }
}