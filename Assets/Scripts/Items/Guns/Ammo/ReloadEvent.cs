using EventBus;

namespace Items.Guns
{
    public class ReloadEvent : IEvent
    {
        public Magazine CurrentMagazine { get; }
            
        public ReloadEvent(Magazine currentMagazine)
        {
            CurrentMagazine = currentMagazine;
        }
            
    }
}