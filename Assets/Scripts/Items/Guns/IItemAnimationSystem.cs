namespace Items.Guns
{
    public interface IItemAnimationSystem
    {
        void PlayAnimation(ItemAnimation animation, float startTime = 0f);
        float PlayAnimationAndGetLength(ItemAnimation animation, float startTime = 0f);
        float GetCurrentAnimationTime();
    }
}