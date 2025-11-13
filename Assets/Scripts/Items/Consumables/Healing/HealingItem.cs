using System;
using Attributes;
using Cysharp.Threading.Tasks;
using Items.Guns;
using Player;
using Shared;
using UnityEngine;

namespace Items.Consumables.Healing
{
    public class HealingItem : Item
    {
        public float healingAmount = 45f;

        [Required] public ItemAnimation useAnimation;

        public GameObject emptySyringePrefab;

        [SerializeField] private int charges = 3;

        private ItemHandler _itemHandler;

        private IKillable _playerKillable;

        private RigHandler _rigHandler;

        public override bool CanBeSwappedFrom => !AnimationPlaying;
        public bool AnimationPlaying { get; private set; }


        private void Awake()
        {
            _rigHandler = GetComponentInParent<RigHandler>();
            ItemAnimationSystem = new PlayerItemAnimationSystem();
            _itemHandler = GetComponentInParent<ItemHandler>();
            _playerKillable = GetComponentInParent<IKillable>();
        }

        public override async void Use()
        {
            if (AnimationPlaying) return;
            _rigHandler.LeftHandFollowItemTarget = false;
            _rigHandler.LeftHandFollowItemHint = false;
            _playerKillable.Health += healingAmount;
            var length = ItemAnimationSystem.PlayAnimationAndGetLength(useAnimation);
            await WaitUntilAnimationComplete(length);
            _rigHandler.LeftHandFollowItemTarget = true;
            _rigHandler.LeftHandFollowItemHint = true;
            SpawnEmptySyringe();
            charges -= 1;
            ItemAnimationSystem.PlayAnimation(holdingItemAnimation);

            if (charges > 0) return;

            _itemHandler.TryRemoveItem(this);
            Destroy(gameObject);
        }

        public async UniTask WaitUntilAnimationComplete(float seconds)
        {
            AnimationPlaying = true;
            await UniTask.Delay(TimeSpan.FromSeconds(seconds));
            AnimationPlaying = false;
        }

        private void SpawnEmptySyringe()
        {
            if (emptySyringePrefab != null) Instantiate(emptySyringePrefab, transform.position, transform.rotation);
        }

        protected override void OnUpdate()
        {
        }

        public override void Equip()
        {
            base.Equip();
            ItemAnimationSystem.PlayAnimationAndGetLength(holdingItemAnimation);
        }
    }
}