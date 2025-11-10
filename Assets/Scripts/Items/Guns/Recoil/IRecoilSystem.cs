namespace Items.Guns.Recoil
{
    public interface IRecoilSystem : IItemSystem
    {
        void ApplyRecoil();

        void Reset();
    }
}