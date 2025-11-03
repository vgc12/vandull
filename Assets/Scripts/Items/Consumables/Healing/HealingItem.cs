using General;
using Shared;

namespace Items.Consumables.Healing
{
    public class HealingItem : Item
    {
        public float healingAmount = 25f;

        public override bool CanBeSwappedFrom { get; } = true;

        public override void Use()
        {
            GetComponentInParent<IKillable>().Health += healingAmount;
        }

        protected override void OnUpdate()
        {
        }

        public override void Equip()
        {
            base.Equip();
         
        }
    }
}