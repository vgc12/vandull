using System;
using Attributes;
using Player;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Items
{
    public abstract class Item : MonoBehaviour, IEquippable
    {
        public bool IsEquipped;

        [SerializeField] private string itemName;

        protected abstract void Use(InputAction.CallbackContext context);

        protected InputManager InputManager;
        protected abstract void OnUpdate();
        public MonoBehaviour MonoBehaviour { get; private set; }

        
        
        public virtual bool OwnedByEnemy { get; set; }


        private void Start()
        {
            
        }


        public void Update()
        {
            if (IsEquipped)
            {
                OnUpdate();
            }
        }


        public virtual void Awake()
        {
      
            if(OwnedByEnemy) return;
            InputManager = GetComponentInParent<InputManager>();
            InputManager.InputActions.Player.Attack.started += Use;
            InputManager.InputActions.Player.Attack.performed += Use;
            InputManager.InputActions.Player.Attack.canceled += Use;
        }


        public virtual void Despawn()
        {
            InputManager.InputActions.Player.Attack.started -= Use;
            InputManager.InputActions.Player.Attack.performed -= Use;
            InputManager.InputActions.Player.Attack.canceled -= Use;
            
            if (gameObject!= null)
            {
                Destroy(gameObject);
            }
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