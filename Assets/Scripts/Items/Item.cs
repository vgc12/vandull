using System;
using Attributes;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;



namespace Items
{
    public abstract class Item : MonoBehaviour
    {
        
        protected bool IsEquipped;
        
        [SerializeField] private string itemName;
        
        protected abstract void Use(InputAction.CallbackContext context);
    
         protected InputManager InputManager;

        private void Awake()
        {
           
            IsEquipped = false;

        }

        private void Start()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            InputManager = GetComponentInParent<InputManager>();
            InputManager.InputActions.Player.Attack.started += Use;
            InputManager.InputActions.Player.Attack.performed += Use;
            InputManager.InputActions.Player.Attack.canceled += Use;
        }
        
        public virtual void Equip()
        {
            gameObject.SetActive(true);
            IsEquipped = true;
        }


        public virtual void UnEquip()
        {
            gameObject.SetActive(false);
            IsEquipped = false;
        }
        
        
    }
}