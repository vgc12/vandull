namespace Items.Guns
{
    public class EnemyGunAnimationSystem : IItemAnimationSystem
    {
        //noop for now
        public void PlayAnimation(ItemAnimation animation)
        {
        }

        public float PlayAnimationAndGetLength(ItemAnimation animation)
        {
            return 0;
        }
    }
}