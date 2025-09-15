using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using General;
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
private MonoBehaviour _behaviour;
        public bool IsCurrentMagazineEmpty => CurrentMagazine.IsEmpty;
        public bool IsReloading => _isReloading;
        public int CurrentAmmo => CurrentMagazine.CurrentAmmo;
        public int TotalAmmo => _magazines.Count * _config.ammoSettings.magazineSize;

        private Magazine CurrentMagazine => _magazines[_currentMagazineIndex];

        public AmmoSystem(GunConfig config, MonoBehaviour behaviour)
        {
            _config = config;
            _behaviour = behaviour;
            InitializeMagazines();
        }

        private void InitializeMagazines()
        {
            for (int i = 0; i < _config.ammoSettings.magazineCount; i++)
            {
                _magazines.Add(new Magazine(_config.ammoSettings.magazineSize));
            }
        }

        public bool CanReload => !_isReloading && HasSpareAmmo;

        private bool HasSpareAmmo => _magazines.Count > 1 ;

        public void StartReload()
        {
            if (!CanReload) return;

            _isReloading = true;
            OnReloadStarted?.Invoke();
            
           _behaviour.StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            yield return new WaitForSeconds(_config.ammoSettings.reloadTime);
            
            if(_magazines[_currentMagazineIndex].CurrentAmmo == 0)
            {
                _magazines.RemoveAt(_currentMagazineIndex);
            }
            
            _currentMagazineIndex = (_currentMagazineIndex + 1) % _magazines.Count;
            _isReloading = false;
            
           VandullLogger.Log(this);
            
            OnAmmoChanged?.Invoke();
            OnReloadCompleted?.Invoke();
        }
        
        public void ConsumeAmmo()
        {
            CurrentMagazine.SubtractOne();
            OnAmmoChanged?.Invoke();
        }

        public override string ToString()
        {
            return GetAllMagsStatus();
        }

        public string GetAllMagsStatus()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(MagazineStatus());
         
            for (int i = 0; i < _magazines.Count; i++)
            {
                stringBuilder.AppendLine(MagazineStatus(i));
                
            }

            return stringBuilder.ToString();
        }

        private string MagazineStatus(int index = -1)
        {
            if (index >= 0 && index < _magazines.Count)
            {
                var mag = _magazines[index];
                return $"Magazine {index + 1}: {mag.CurrentAmmo}/{_config.ammoSettings.magazineSize}";
            }

            return
                $"{CurrentMagazine.CurrentAmmo}/{_config.ammoSettings.magazineSize} | Magazines: {_magazines.Count}";
        }
    }
}