using System;
using System.Collections.Generic;
using System.Linq;
using General;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items
{
    public class ItemHandler : MonoBehaviour
    {

        private Item _equippedItem;
       [SerializeField] private List<Item> inventory = new();
        private InputManager _inputManager;

        private void Start()
        {
            inventory = GetComponentsInChildren<Item>().ToList();
            LogInventory();
            _equippedItem = inventory.FirstOrDefault();
            _equippedItem?.Equip();
            _inputManager = GetComponentInParent<InputManager>();
            _inputManager.InputActions.Player.SwitchItem.performed += OnItemSwitched;
            UnequipAllButSelected();
        }

        private void LogInventory()
        {
            Debug.Log("Current Inventory:");
            foreach (var item in inventory)
            {
               VandullLogger.Log(item.name + (item == _equippedItem ? " (Equipped)" : ""));
            }
        }
        
        public void UnequipAllButSelected()
        {
            foreach (var i in inventory)
            {
                if(i == _equippedItem)
                   continue;
                i.UnEquip();
            }
        }
  

        private void OnItemSwitched(InputAction.CallbackContext obj)
        {
            var direction = obj.ReadValue<float>();
            if (inventory.Count == 0) return;
            int currentIndex = inventory.IndexOf(_equippedItem);
            int nextIndex = Math.Abs((currentIndex + (int)direction) % inventory.Count);
            EquipItem(inventory[nextIndex]);
        }
        
        private void EquipItem(Item item)
        {
            if (_equippedItem != null)
                _equippedItem.UnEquip();
            _equippedItem = item;
            _equippedItem.Equip();
        }
    }
}
