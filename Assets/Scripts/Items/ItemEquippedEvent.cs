using EventBus;

namespace Items
{
    /// <summary>
    /// Event raised when an item is equipped by the player.
    /// </summary>
    public sealed class ItemEquippedEvent : IEvent
    {
        /// <summary>
        /// Initializes a new instance of the ItemEquippedEvent.
        /// </summary>
        /// <param name="item">The item that was equipped.</param>
        public ItemEquippedEvent(Item item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the item that was equipped.
        /// </summary>
        public Item Item { get; }
    }
}