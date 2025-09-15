using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using General;
using Items.Guns;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items
{
    public class ItemHandler : MonoBehaviour
    {

        private Item _equippedItem;
       [SerializeField, Required] private List<Item> inventory = new();
        private InputManager _inputManager;

        private void Start()
        {
            LogInventory();
            SetUpItems();
            _equippedItem = inventory.FirstOrDefault();
            _equippedItem?.Equip();
            _inputManager = GetComponentInParent<InputManager>();
            _inputManager.InputActions.Player.SwitchItem.performed += OnItemSwitched;
     
        }

        private void LogInventory()
        {
            Debug.Log("Current Inventory:");
            foreach (var item in inventory)
            {
               VandullLogger.Log(item.name + (item == _equippedItem ? " (Equipped)" : ""));
            }
        }
        
        public void SetUpItems()
        {
            foreach (var i in inventory)
            {
                InitializeItem(i);
                if(i == _equippedItem)
                   continue;
                i.UnEquip();
            }
        }

        public void InitializeItem(Item item)
        {
            item.Spawn(this);
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

        private void Update()
        {
            _equippedItem.Update();
        }

        private void OnDrawGizmos()
        {
            (_equippedItem as Gun)?.OnDrawGizmos();
        }
    }
}
