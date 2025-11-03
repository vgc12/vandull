using Npcs.Shared;

namespace Items.Guns
{
    public class PlayerGunAnimationSystem : IItemAnimationSystem
    {
        public void PlayAnimation(ItemAnimation animation, float startTime = 0f)
        {
            ArmAnimationController.Instance.PlayAnimation(animation, startTime);
        }

        public void CrossFadeToAnimation(ItemAnimation animation, float duration, float startTime = 0)
        {
            ArmAnimationController.Instance.CrossFadeToAnimation(animation, duration, startTime);
        }

        public float PlayAnimationAndGetLength(ItemAnimation animation, float startTime = 0f)
        {
            return ArmAnimationController.Instance.PlayAnimation(animation, startTime);
        }

        public float GetCurrentAnimationTime()
        {
            return ArmAnimationController.Instance.GetCurrentAnimationTime();
        }


        public void Update()
        {
        }
    }
}