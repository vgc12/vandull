using Player;
using UnityEngine;

namespace Items
{
    public abstract class Item : MonoBehaviour, IEquippable
    {
        [SerializeField] private string itemName;

        protected InputManager InputManager;
        public bool IsEquipped { get; protected set; }

        public void Update()
        {
            if (IsEquipped) OnUpdate();
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

        protected abstract void OnUpdate();

        public virtual void Despawn()
        {
            if (gameObject != null) Destroy(gameObject);
        }
    }
}