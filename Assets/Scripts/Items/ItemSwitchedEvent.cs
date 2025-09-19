using EventBus;

namespace Items
{
    public class ItemSwitchedEvent : IEvent
    {
        public Item NewItem { get; set; }
        public ItemSwitchedEvent(Item newItem)
        {
            NewItem = newItem;
        }
    
    }
}