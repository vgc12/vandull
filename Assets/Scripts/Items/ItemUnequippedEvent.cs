using EventBus;

namespace Items
{
    /// <summary>
    /// Event raised when an item is unequipped by the player.
    /// </summary>
    public class ItemUnequippedEvent : IEvent
    {
        /// <summary>
        /// Initializes a new instance of the ItemUnequippedEvent.
        /// </summary>
        /// <param name="item">The item that was unequipped.</param>
        public ItemUnequippedEvent(Item item)
        {
            Item = item;
        }

        /// <summary>
        /// Gets the item that was unequipped.
        /// </summary>
        public Item Item { get; }
    }
}