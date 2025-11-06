namespace Items.Guns
{
    public class EnemyGunAnimationSystem : IItemAnimationSystem
    {
        //noop for now


        public void PlayAnimation(ItemAnimation animation, float startTime = 0)
        {
        }

        public void CrossFadeToAnimation(ItemAnimation animation, float duration, float startTime = 0)
        {
        }

        public float PlayAnimationAndGetLength(ItemAnimation animation, float startTime = 0)
        {
            return 0;
        }

        public float GetCurrentAnimationTime()
        {
            return 0;
        }

        public void Update()
        {
        }
    }
}