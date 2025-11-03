using Attributes;
using EventBus;
using Items.Guns;
using UnityEngine;

namespace Items
{
    /// <summary>
    ///     Base class for all equippable items in the game.
    ///     Handles equipment state, IK targets, animations, and ownership.
    /// </summary>
    public abstract class Item : MonoBehaviour, IEquippable
    {
        #region Identification

        /// <summary>
        ///     Display name of the item.
        /// </summary>
        [SerializeField] [Tooltip("Display name of the item")]
        private string itemName;

        #endregion

        #region Animation

        /// <summary>
        ///     Animation data for holding this item (idle, walking, running animations).
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("Animation data for holding this item (idle, walking, running animations)")]
        public ItemAnimation holdingItemAnimation;

        #endregion

        #region Systems

        /// <summary>
        ///     System responsible for managing item animations and transitions.
        /// </summary>
        public IItemAnimationSystem ItemAnimationSystem;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        ///     Updates the item each frame if it is currently equipped.
        /// </summary>
        public void Update()
        {
            if (IsEquipped) OnUpdate();
        }

        #endregion

        #region Lifecycle

        /// <summary>
        ///     Despawns and destroys this item from the game world.
        /// </summary>
        public virtual void Despawn()
        {
            if (gameObject != null) Destroy(gameObject);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        ///     Called every frame while the item is equipped.
        ///     Override to implement item-specific update logic.
        /// </summary>
        protected abstract void OnUpdate();

        public virtual void Use()
        {
        }

        public virtual void StopUse()
        {
        }

        #endregion

        #region IK Targets

        /// <summary>
        ///     Target transform for the left hand IK position.
        /// </summary>
        [SerializeField] [Tooltip("Target transform for the left hand IK position")]
        public Transform leftHandTarget;

        /// <summary>
        ///     Hint transform for the left hand IK elbow positioning.
        /// </summary>
        [SerializeField] [Tooltip("Hint transform for the left hand IK elbow positioning")]
        public Transform leftHandHint;

        /// <summary>
        ///     Target transform for the right hand IK position.
        /// </summary>
        [SerializeField] [Tooltip("Target transform for the right hand IK position")]
        public Transform rightHandTarget;

        /// <summary>
        ///     Hint transform for the right hand IK elbow positioning.
        /// </summary>
        [SerializeField] [Tooltip("Hint transform for the right hand IK elbow positioning")]
        public Transform rightHandHint;

        #endregion

        #region State

        /// <summary>
        ///     Gets or sets who currently owns this item (Player, AI, or None).
        /// </summary>
        public OwnerStatus Owner { get; set; }

        /// <summary>
        ///     Gets whether this item is currently equipped and active.
        /// </summary>
        public bool IsEquipped { get; protected set; }

        /// <summary>
        ///     Indicates if the item can be swapped to another item.
        /// </summary>
        public abstract bool CanBeSwappedFrom { get; }

        #endregion

        #region Equipment

        /// <summary>
        ///     Equips the item, making it active and visible.
        ///     Raises an ItemEquippedEvent if owned by the player.
        /// </summary>
        public virtual void Equip()
        {
            gameObject.SetActive(true);
            IsEquipped = true;

            if (Owner == OwnerStatus.Player) EventBus<ItemEquippedEvent>.Raise(new ItemEquippedEvent(this));
        }

        /// <summary>
        ///     Unequips the item, making it inactive and hidden.
        ///     Raises an ItemUnequippedEvent if owned by the player.
        /// </summary>
        public virtual void UnEquip()
        {
            gameObject.SetActive(false);
            IsEquipped = false;

            if (Owner == OwnerStatus.Player) EventBus<ItemUnequippedEvent>.Raise(new ItemUnequippedEvent(this));
        }

        #endregion
    }
}