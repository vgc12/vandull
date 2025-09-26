
using Items.Guns;
using Player;

using UnityEngine;
using UnityEngine.InputSystem;


namespace Items
{
    public abstract class Item : MonoBehaviour, IEquippable
    {
        public bool IsEquipped { get; protected set; }

        [SerializeField] private string itemName;

        protected InputManager InputManager;
        protected abstract void OnUpdate();
        
        public void Update()
        {
            if (IsEquipped)
            {
                
                OnUpdate();
            }
        }

        public virtual void Despawn()
        {
          
            
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