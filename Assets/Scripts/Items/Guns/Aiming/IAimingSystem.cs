namespace Items.Guns.Aiming
{
    public interface IAimingSystem : IGunSystem
    {
        bool IsAiming { get; }

        void StartAiming();

        void StopAiming();
    }
}