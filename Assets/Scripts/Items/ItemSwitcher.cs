using EventBus;
using Player.Input;
using Reflex.Attributes;
using Singletons;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(ItemHandler))]
    public class ItemSwitcher : Singleton<ItemSwitcher>
    {
        [Inject] private readonly IPlayerInput _input;

        private ItemHandler _itemHandler;

        private EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;

        private void Start()
        {
            _input.SwitchItem += OnItemSwitched;
            _itemHandler = GetComponent<ItemHandler>();
        }

        private void OnDestroy()
        {
            _input.SwitchItem -= OnItemSwitched;
        }

        private void OnItemSwitched(float value)
        {
            _itemHandler.SwitchItem((int)value);
        }
    }
}