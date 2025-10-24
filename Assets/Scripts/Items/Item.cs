using System;
using Items.Guns;
using Player;
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

        protected InputManager InputManager;

        public GripType gripType = GripType.Pistol;
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

        
        
        private void OnDestroy()
        {
            Despawn();
        }
    }
}