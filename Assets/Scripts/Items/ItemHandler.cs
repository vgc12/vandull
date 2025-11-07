using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using EventBus;
using Items.Guns;
using Player;
using Reflex.Attributes;
using UnityEngine;
using ILogger = General.Logging.ILogger;


namespace Items
{
    [Serializable]
    public class ItemHandler : MonoBehaviour
    {
        [SerializeField] public List<Gun> gunObjects = new();

        [SerializeField] [Required] private RigHandler rigHandler;

        private List<Item> _inventory = new();

        [Inject] public ILogger Logger;

        public Item EquippedItem { get; private set; }

        public List<Item> Inventory
        {
            get => _inventory;
            private set => _inventory = value;
        }


        // Call this from Awake() or Start() in your MonoBehaviour
        public void Start()
        {
            gunObjects ??= new List<Gun>();

            _inventory ??= new List<Item>();


            SetUpItems();
            LogPrefabs();


            EquipItem(Inventory.FirstOrDefault());

            EventBus<ItemSwitchedEvent>.Raise(new ItemSwitchedEvent(EquippedItem));
        }


        private void LogPrefabs()
        {
            Debug.Log("Current Prefabs:");
            foreach (var prefab in gunObjects) Logger.Log(prefab.name);
        }

        private void LogInventory()
        {
            Debug.Log("Current Inventory:");
            foreach (var item in _inventory) Logger.Log(item.name + (item == EquippedItem ? " (Equipped)" : ""));
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


        public void SwitchItem(int direction)
        {
            // Something is preventing item swap (i.e reloading, mid grenade throw, etc)
            if (!EquippedItem.CanBeSwappedFrom) return;

            if (_inventory.Count == 0) return;
            var currentIndex = _inventory.IndexOf(EquippedItem);
            var nextIndex = Math.Abs((currentIndex + direction) % gunObjects.Count);
            EquipItem(_inventory[nextIndex]);
        }


        private void EquipItem(Item item)
        {
            if (EquippedItem != null) EquippedItem.UnEquip();


            EquippedItem = item;
            EquippedItem.Equip();

            rigHandler.LeftHandTarget = EquippedItem.leftHandTarget;
            rigHandler.LeftHandHint = EquippedItem.leftHandHint;
            rigHandler.RightHandTarget = EquippedItem.rightHandTarget;
            rigHandler.RightHandHint = EquippedItem.rightHandHint;
            rigHandler.RebuildRigs();
        }
    }
}