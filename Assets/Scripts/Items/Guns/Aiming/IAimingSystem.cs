namespace Items.Guns.Aiming
{
    public interface IAimingSystem : IItemSystem
    {
        bool IsAiming { get; }

        void StartAiming();

        void StopAiming();
        void ResetPosition();
    }
}