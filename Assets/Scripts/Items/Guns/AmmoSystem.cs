using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Guns
{
    public class AmmoSystem : IAmmoSystem
    {
        public event Action OnAmmoChanged;
        public event Action OnReloadStarted;
        public event Action OnReloadCompleted;

        private readonly GunConfig _config;
        private readonly List<Magazine> _magazines = new List<Magazine>();
        private int _currentMagazineIndex;
        private bool _isReloading;

        public bool IsCurrentMagazineEmpty => CurrentMagazine.IsEmpty;
        public bool IsReloading => _isReloading;
        public int CurrentAmmo => CurrentMagazine.CurrentAmmo;
        public int TotalAmmo => _magazines.Count * _config.ammoSettings.magazineSize;

        private Magazine CurrentMagazine => _magazines[_currentMagazineIndex];

        public AmmoSystem(GunConfig config)
        {
            _config = config;
            InitializeMagazines();
        }

        private void InitializeMagazines()
        {
            for (int i = 0; i < _config.ammoSettings.magazineCount; i++)
            {
                _magazines.Add(new Magazine(_config.ammoSettings.magazineSize));
            }
        }

        public bool CanReload() => !_isReloading && HasSpareAmmo();

        private bool HasSpareAmmo() => _magazines.Count > 1 || !CurrentMagazine.IsFull;

        public void StartReload()
        {
            if (!CanReload()) return;

            _isReloading = true;
            OnReloadStarted?.Invoke();
            
            CoroutineRunner.StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            yield return new WaitForSeconds(_config.ammoSettings.reloadTime);
            
            _currentMagazineIndex = (_currentMagazineIndex + 1) % _magazines.Count;
            _isReloading = false;
            
            OnAmmoChanged?.Invoke();
            OnReloadCompleted?.Invoke();
        }

        public void ConsumeAmmo()
        {
            CurrentMagazine.SubtractOne();
            OnAmmoChanged?.Invoke();
        }
    }
}