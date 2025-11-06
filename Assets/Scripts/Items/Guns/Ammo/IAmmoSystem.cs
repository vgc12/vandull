using System;

namespace Items.Guns.Ammo
{
    public interface IAmmoSystem : IItemSystem
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

        public Magazine CurrentMagazine { get; }

        void StartReload();

        void StartQuickReload();

        void ConsumeAmmo();

        void DropMagazine();

        void EquipNewMagazine();
        void RemoveCurrentMagazine();
    }
}