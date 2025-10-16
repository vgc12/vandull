using EventBus;
using Player;
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


        private void Start()
        {
            InputManager.Instance.InputActions.Player.SwitchItem.performed += OnItemSwitched;
            _itemHandler = GetComponent<ItemHandler>();
            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(_itemHandler.EquippedItem));
        }

        private void OnDestroy()
        {
            InputManager.Instance.InputActions.Player.SwitchItem.performed -= OnItemSwitched;
        }

        private void OnItemSwitched(InputAction.CallbackContext obj)
        {
            _itemHandler.SwitchItem((int)obj.ReadValue<float>());
            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(_itemHandler.EquippedItem));
        }
    }
}