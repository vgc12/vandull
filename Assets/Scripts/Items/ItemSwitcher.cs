using System;
using EventBus;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items
{
    [RequireComponent(typeof(ItemHandler))]
    public class ItemSwitcher : MonoBehaviour
    {
        public static ItemSwitcher Instance { get; private set; }
        private ItemHandler _itemHandler;
        private InputManager _inputManager;
        
        private EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
               
            }
        }

        private void Start()
        {
            _inputManager = GetComponentInParent<InputManager>();
            _inputManager.InputActions.Player.SwitchItem.performed += OnItemSwitched;
            _itemHandler = GetComponent<ItemHandler>();
            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(_itemHandler.EquippedItem));
        }

        private void OnItemSwitched(InputAction.CallbackContext obj)
        {
            _itemHandler.SwitchItem((int)obj.ReadValue<float>());
            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(_itemHandler.EquippedItem));
        }

        private void OnDisable()
        {
            
            _inputManager.InputActions.Player.SwitchItem.performed -= OnItemSwitched;
        }
    }
}