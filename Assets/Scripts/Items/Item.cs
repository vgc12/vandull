using System;
using Attributes;
using Player;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Items
{
    public abstract class Item : ScriptableObject, IEquippable
    {
        protected bool IsEquipped;

        [SerializeField] private string itemName;

        protected abstract void Use(InputAction.CallbackContext context);

        protected InputManager InputManager;
        protected abstract void OnUpdate();
        protected MonoBehaviour MonoBehaviour;
        [SerializeField] protected GameObject prefab;
        protected GameObject ItemInstance { get; private set; }
        
        
        public virtual bool OwnedByEnemy { get; set; }
        

        


        public void Update()
        {
            if (IsEquipped)
            {
                OnUpdate();
            }
        }


        public virtual void Spawn(MonoBehaviour monoBehaviour, bool ownedByEnemy = false)
    
        {
            MonoBehaviour = monoBehaviour;
            ItemInstance = Instantiate(prefab, monoBehaviour.transform);
            OwnedByEnemy = ownedByEnemy;
            if(OwnedByEnemy) return;
            InputManager = monoBehaviour.GetComponentInParent<InputManager>();
            InputManager.InputActions.Player.Attack.started += Use;
            InputManager.InputActions.Player.Attack.performed += Use;
            InputManager.InputActions.Player.Attack.canceled += Use;
        }


        public virtual void Despawn()
        {
            InputManager.InputActions.Player.Attack.started -= Use;
            InputManager.InputActions.Player.Attack.performed -= Use;
            InputManager.InputActions.Player.Attack.canceled -= Use;
            
            if (ItemInstance != null)
            {
                Destroy(ItemInstance);
            }
        }

        public virtual void Equip()
        {
            ItemInstance.SetActive(true);
            IsEquipped = true;
        }


        public virtual void UnEquip()
        {
            ItemInstance.SetActive(false);
            IsEquipped = false;
        }
        
        
    }
}