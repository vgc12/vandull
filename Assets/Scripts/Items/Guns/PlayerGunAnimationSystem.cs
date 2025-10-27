using Npcs.Shared;

namespace Items.Guns
{
    public class PlayerGunAnimationSystem : IItemAnimationSystem
    {
        public void PlayAnimation(ItemAnimation animation)
        {
            ArmAnimationController.Instance.PlayAnimation(animation);
        }

        public float PlayAnimationAndGetLength(ItemAnimation animation)
        {
            return ArmAnimationController.Instance.PlayAnimation(animation);
        }
    }
}