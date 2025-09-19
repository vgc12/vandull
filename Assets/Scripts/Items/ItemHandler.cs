using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using EventBus;
using General;
using Items.Guns;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class ItemHandler : MonoBehaviour
    {
   
        private Item _equippedItem; 
        public Item EquippedItem => _equippedItem;
        public List<Item> inventory = new List<Item>();
   

       

   

        // Call this from Awake() or Start() in your MonoBehaviour
        public void Start()
        {
            inventory ??= new List<Item>();
            
       
            LogInventory();
            SetUpItems();
            
            _equippedItem = inventory.FirstOrDefault();
            _equippedItem?.Equip();
            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(_equippedItem));
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
         
        }
  
        public void SwitchItem(int direction)
        {
            if (inventory.Count == 0) return;
            int currentIndex = inventory.IndexOf(_equippedItem);
            int nextIndex = Math.Abs((currentIndex + direction) % inventory.Count);
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
