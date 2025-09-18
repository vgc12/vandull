namespace Items.Guns.Recoil
{
    public interface IRecoilSystem
    {
        void ApplyRecoil();
        void Update();
        void Reset();
    }
}