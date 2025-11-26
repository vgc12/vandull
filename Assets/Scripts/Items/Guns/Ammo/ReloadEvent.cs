using EventBus;

namespace Items.Guns.Ammo
{
    public sealed class ReloadEvent : IEvent
    {
        public ReloadEvent(Magazine currentMagazine)
        {
            CurrentMagazine = currentMagazine;
        }

        public Magazine CurrentMagazine { get; }
    }
}