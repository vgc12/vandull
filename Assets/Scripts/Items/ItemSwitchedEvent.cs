using EventBus;

namespace Items
{
    public sealed class ItemSwitchedEvent : IEvent
    {
        public ItemSwitchedEvent(Item newItem)
        {
            NewItem = newItem;
        }

        public Item NewItem { get; set; }
    }
}