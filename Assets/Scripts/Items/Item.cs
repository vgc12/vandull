using Attributes;
using EventBus;
using Items.Guns;
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

        [Required] public ItemAnimation holdingItemAnimation;

        public IItemAnimationSystem ItemAnimationSystem;

        public OwnerStatus Owner { get; set; }


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
            if (Owner == OwnerStatus.Player) EventBus<ItemEquippedEvent>.Raise(new ItemEquippedEvent(this));
        }


        public virtual void UnEquip()
        {
            gameObject.SetActive(false);
            IsEquipped = false;
            if (Owner == OwnerStatus.Player) EventBus<ItemUnequippedEvent>.Raise(new ItemUnequippedEvent(this));
        }

        protected abstract void OnUpdate();

        public virtual void Despawn()
        {
            if (gameObject != null) Destroy(gameObject);
        }
    }

    public class ItemEquippedEvent : IEvent
    {
        public ItemEquippedEvent(Item item)
        {
            Item = item;
        }

        public Item Item { get; }
    }

    public class ItemUnequippedEvent : IEvent
    {
        public ItemUnequippedEvent(Item item)
        {
            Item = item;
        }

        public Item Item { get; }
    }
}