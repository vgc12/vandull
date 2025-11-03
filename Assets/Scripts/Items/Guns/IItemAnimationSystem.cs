namespace Items.Guns
{
    public interface IItemAnimationSystem : IItemSystem
    {
        void PlayAnimation(ItemAnimation animation, float startTime = 0f);
        void CrossFadeToAnimation(ItemAnimation animation, float duration, float startTime = 0f);
        float PlayAnimationAndGetLength(ItemAnimation animation, float startTime = 0f);
        float GetCurrentAnimationTime();
    }
}