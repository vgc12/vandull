using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using EventBus;
using General;
using Items.Guns;
using Items.Guns.Items.Guns;
using Items.Guns.Items.Guns.Builder;
using Items.Guns.Items.Guns.Dependencies;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class ItemHandler : MonoBehaviour
    {
   
        private Item _equippedItem; 
        public Item EquippedItem => _equippedItem;
        [SerializeField, Required] public List<Gun> gunObjects = new List<Gun>();
        private List<Item> _inventory = new List<Item>();
        public List<Item> Inventory
        {
            get => _inventory;
            private set => _inventory = value;
        }

   

        // Call this from Awake() or Start() in your MonoBehaviour
        public void Awake()
        {
            gunObjects ??= new List<Gun>();
            
            _inventory ??= new List<Item>();

            SetUpItems();
            LogPrefabs();
    
            
            _equippedItem = _inventory.FirstOrDefault();
       
            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(_equippedItem));
        }

        private void Start()
        {
            foreach (var i in _inventory)
            {
                if (i is not Gun gun) continue;
                
             
                var builder = new Gun.Initializer(gun);
                builder.ForPlayer().Initialize();
                
              
            }
            _equippedItem?.Equip();
        }

        private void LogPrefabs()
        {
            Debug.Log("Current Prefabs:");
            foreach (var prefab in gunObjects)
            {
                VandullLogger.Log(prefab.name);
            }
        }
        
        private void LogInventory()
        {
            Debug.Log("Current Inventory:");
            foreach (var item in _inventory)
            {
               VandullLogger.Log(item.name + (item == _equippedItem ? " (Equipped)" : ""));
            }
        }
        
        public void SetUpItems()
        {
            gunObjects = GetComponentsInChildren<Gun>().ToList();
            foreach (var i in gunObjects)
            {
                _inventory.Add(i);
            }
        }

        public void InitializeItem(Item item)
        {
         
        }
  
        public void SwitchItem(int direction)
        {
            if (_inventory.Count == 0) return;
            int currentIndex = _inventory.IndexOf(_equippedItem);
            int nextIndex = Math.Abs((currentIndex + direction) % gunObjects.Count);
            EquipItem(_inventory[nextIndex]);
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
