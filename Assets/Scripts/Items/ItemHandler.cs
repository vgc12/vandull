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
        [SerializeField] public List<Gun> gunObjects = new();

        private List<Item> _inventory = new();

        [Required] private RigHandler _rigHandler;
        
        public Item EquippedItem { get; private set; }

        [Header("Arm Animations")]
        public Animator Animator;
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
            
            _rigHandler = GetComponent<RigHandler>();

            SetUpItems();
            LogPrefabs();


            EquipItem(Inventory.FirstOrDefault());

            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(EquippedItem));
        }

        private void Start()
        {
   
        }

        private void LogPrefabs()
        {
            Debug.Log("Current Prefabs:");
            foreach (var prefab in gunObjects) VandullLogger.Log(prefab.name);
        }

        private void LogInventory()
        {
            Debug.Log("Current Inventory:");
            foreach (var item in _inventory) VandullLogger.Log(item.name + (item == EquippedItem ? " (Equipped)" : ""));
        }

        public void SetUpItems()
        {
            gunObjects = GetComponentsInChildren<Gun>().ToList();
            foreach (var i in gunObjects)
            {
                _inventory.Add(i);
                i.UnEquip();
            }
        }

        public void InitializeItem(Item item)
        {
        }

        public void SwitchItem(int direction)
        {
            if (_inventory.Count == 0) return;
            var currentIndex = _inventory.IndexOf(EquippedItem);
            var nextIndex = Math.Abs((currentIndex + direction) % gunObjects.Count);
            EquipItem(_inventory[nextIndex]);
        }


        private void EquipItem(Item item)
        {
            if (EquippedItem != null)
            {
                EquippedItem.UnEquip();
                
            }

         
            EquippedItem = item;

            _rigHandler.SetLeftHandData(EquippedItem.leftHandTarget, EquippedItem.leftHandHint);
            _rigHandler.SetRightHandData(EquippedItem.rightHandTarget, EquippedItem.rightHandHint);
            
            Animator.SetLayerWeight((int)EquippedItem.gripType, 1);
            EquippedItem.Equip();
        }
    }
}