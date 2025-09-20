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
        [SerializeField, Required] public List<GameObject> prefabs = new List<GameObject>();
        private List<Item> _inventory = new List<Item>();
        public List<Item> Inventory
        {
            get => _inventory;
            private set => _inventory = value;
        }
        
        
        [Required, SerializeField] private Transform hipFireTransform;
        [Required, SerializeField] private Transform adsTransform;
        [Required, SerializeField] private Transform recoilTransform;

   

        // Call this from Awake() or Start() in your MonoBehaviour
        public void Awake()
        {
            prefabs ??= new List<GameObject>();
            
            _inventory ??= new List<Item>();

            SetUpItems();
            LogPrefabs();
         //   LogInventory();
            
            _equippedItem = _inventory.FirstOrDefault();
       
            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(_equippedItem));
        }

        private void Start()
        {
            _inventory.ForEach(i => i.Initialize(new GunInitializationData(hipFireTransform, adsTransform, recoilTransform)));
          _equippedItem?.Equip();
        }

        private void LogPrefabs()
        {
            Debug.Log("Current Prefabs:");
            foreach (var prefab in prefabs)
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
            foreach (var i in prefabs)
            {
                
                var obj= Instantiate(i);
                var item = obj.GetComponent<Item>();
                _inventory.Add(item);
                
                if(item is Gun gun)
                {
                    gun.hipFireTransform = hipFireTransform;
                    gun.adsTransform = adsTransform;
                    gun.recoilTransform = recoilTransform;
                }
                
                item.transform.SetParent(transform);
                
                item.UnEquip();
               
              
            }
        }

        public void InitializeItem(Item item)
        {
         
        }
  
        public void SwitchItem(int direction)
        {
            if (_inventory.Count == 0) return;
            int currentIndex = _inventory.IndexOf(_equippedItem);
            int nextIndex = Math.Abs((currentIndex + direction) % prefabs.Count);
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
