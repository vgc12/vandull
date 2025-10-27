namespace Items.Guns
{
    public interface IItemAnimationSystem
    {
        void PlayAnimation(ItemAnimation animation);
        float PlayAnimationAndGetLength(ItemAnimation animation);
    }
}