using EventBus;

namespace Items
{
    internal class ItemSwitchedEvent : IEvent
    {
        public Item NewItem { get; set; }
        public ItemSwitchedEvent(Item newItem)
        {
            NewItem = newItem;
        }
    
    }
}