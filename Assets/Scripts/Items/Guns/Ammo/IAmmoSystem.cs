using System;

namespace Items.Guns.Ammo
{
    public interface IAmmoSystem : IGunSystem
    {
        // When the magazine is empty but may have chambered round

        bool IsReloading { get; }
        bool CanReload { get; }
        int CurrentAmmo { get; }

        int TotalAmmo { get; }

        // Completely out of all ammo
        bool OutOfAmmo { get; }

        public Action<ReloadEvent> OnReloadComplete { get; set; }

        public Action OnOutOfAmmo { get; set; }
        void StartReload();
        void ConsumeAmmo();

        void DropMagazine();
    }
}