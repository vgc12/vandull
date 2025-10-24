using EventBus;
using Player;
using Player.Input;
using Reflex.Attributes;
using Singletons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items
{
    [RequireComponent(typeof(ItemHandler))]
    public class ItemSwitcher : Singleton<ItemSwitcher>
    {
        private ItemHandler _itemHandler;

        private EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;

        [Inject]
        private readonly IPlayerInput _input;

        private void Start()
        {
            _input.SwitchItem += OnItemSwitched;
            _itemHandler = GetComponent<ItemHandler>();
            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(_itemHandler.EquippedItem));
        }

        private void OnDestroy()
        {
            _input.SwitchItem -= OnItemSwitched;
        }

        private void OnItemSwitched(float value)
        {
            _itemHandler.SwitchItem((int)value);
            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(_itemHandler.EquippedItem));
        }
    
    }
}