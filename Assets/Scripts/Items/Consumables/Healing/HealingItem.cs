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
        public float healingAmount = 25f;

        [Required] public ItemAnimation useAnimation;

        public GameObject emptySyringePrefab;

        private RigHandler _rigHandler;

        public override bool CanBeSwappedFrom { get; } = true;

        private void Awake()
        {
            _rigHandler = GetComponentInParent<RigHandler>();
            ItemAnimationSystem = new PlayerItemAnimationSystem();
        }

        public override async void Use()
        {
            _rigHandler.LeftHandFollowItemTarget = false;
            GetComponentInParent<IKillable>().Health += healingAmount;
            var length = ItemAnimationSystem.PlayAnimationAndGetLength(useAnimation);
            await WaitUntilAnimationComplete(length);
        }

        public async UniTask WaitUntilAnimationComplete(float seconds)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds));
            _rigHandler.LeftHandFollowItemTarget = true;
            SpawnEmptySyringe();
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
            ItemAnimationSystem.PlayAnimationAndGetLength(useAnimation);
        }
    }
}