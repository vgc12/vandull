namespace Items.Guns.Recoil
{
    public interface IRecoilSystem : IGunSystem
    {
        void ApplyRecoil();

        void Reset();
    }
}