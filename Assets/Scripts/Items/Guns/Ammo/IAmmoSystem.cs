using System;

namespace Items.Guns
{
    public interface IAmmoSystem : IGunSystem
    {
   

        
        bool CurrentMagazineEmpty { get; }
        bool IsReloading { get; }
        bool CanReload { get; }
        void StartReload();
        void ConsumeAmmo();
        int CurrentAmmo { get; }
        int TotalAmmo { get; }
        
        void DropMagazine();

        public event Action<ReloadEvent> OnReloadComplete;
        public event Action OnOutOfAmmo;
    }
}