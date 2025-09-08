using Attributes;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items
{
    public abstract class Item<T> : MonoBehaviour where T : ItemConfig
    {

        [SerializeField, Required, ScriptableObjectDropdown]
        protected T itemConfig;
    
        protected abstract void Use(InputAction.CallbackContext context);
    
        [HideInInspector]  public InputManager inputManager;

        protected virtual void Initialize()
        {
            inputManager = GetComponentInParent<InputManager>();
            inputManager.InputActions.Player.Attack.performed += Use;
            
        }
    
        private void Start()
        {
            Initialize(); 
        }
    }
}