using System;
using Items.Guns;
using Player.Input;
using UnityEngine;

namespace Items
{
    public abstract class Item : MonoBehaviour, IEquippable
    {
        [SerializeField] private string itemName;

        public Transform leftHandTarget;

        public Transform leftHandHint;

        public Transform rightHandTarget;

        public Transform rightHandHint;

        public GripType gripType = GripType.Pistol;

        protected InputManager InputManager;
        public Action OnItemEquipped;
        public Action OnItemUnequipped;
        public bool IsEquipped { get; protected set; }

        public void Update()
        {
            if (IsEquipped) OnUpdate();
        }


        public virtual void OnDestroy()
        {
        }

        public virtual void Equip()
        {
            gameObject.SetActive(true);
            IsEquipped = true;
            OnItemEquipped?.Invoke();
        }


        public virtual void UnEquip()
        {
            gameObject.SetActive(false);
            IsEquipped = false;
            OnItemUnequipped?.Invoke();
        }

        protected abstract void OnUpdate();

        public virtual void Despawn()
        {
            if (gameObject != null) Destroy(gameObject);
        }
    }
}