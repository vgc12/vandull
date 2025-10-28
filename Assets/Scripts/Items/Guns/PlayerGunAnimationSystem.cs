using Npcs.Shared;

namespace Items.Guns
{
    public class PlayerGunAnimationSystem : IItemAnimationSystem
    {
        public void PlayAnimation(ItemAnimation animation, float startTime = 0f)
        {
            ArmAnimationController.Instance.PlayAnimation(animation);
        }

        public float PlayAnimationAndGetLength(ItemAnimation animation, float startTime = 0f)
        {
            return ArmAnimationController.Instance.PlayAnimation(animation);
        }

        public float GetCurrentAnimationTime()
        {
            return ArmAnimationController.Instance.GetCurrentAnimationTime();
        }
    }
}