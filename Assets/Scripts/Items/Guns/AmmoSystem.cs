using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using General;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Items.Guns
{
    public class AmmoSystem : IAmmoSystem
    {
        public event Action OnAmmoChanged;
        public event Action OnReloadStarted;
        public event Action OnReloadCompleted;

        private readonly GunConfig _config;
        private readonly List<Magazine> _magazines = new();
        private int _currentMagazineIndex;
        private bool _isReloading;
        private MonoBehaviour _behaviour;
        public bool IsCurrentMagazineEmpty => CurrentMagazine.IsEmpty;
        public bool IsReloading => _isReloading;
        public int CurrentAmmo => CurrentMagazine.CurrentAmmo;
        public int TotalAmmo => _magazines.Count * _config.ammoSettings.magazineSize;

        private Magazine CurrentMagazine => _magazines[_currentMagazineIndex];
        
        private ObjectPool<Magazine> _magazinePool;

        private GameObject _magazinePrefab;
        
        private Transform _gunTransform;
        
        public AmmoSystem(GunConfig config, MonoBehaviour behaviour, Transform gunTransform, GameObject magazinePrefab)
        {
            _config = config;
            _gunTransform = gunTransform;
            _behaviour = behaviour;
            _magazinePrefab = magazinePrefab;
            _magazinePool = new ObjectPool<Magazine>(CreateMagazine);
            InitializeMagazines();
          
        }

        private Magazine CreateMagazine()
        {
           var magObject = Object.Instantiate(_magazinePrefab);
      
            var rigidbody = magObject.GetOrAddComponent<Rigidbody>();
            var collider = magObject.GetOrAddComponent<BoxCollider>();
            var mr = magObject.GetOrAddComponent<MeshRenderer>();
            var magazine = magObject.GetOrAddComponent<Magazine>();
            magazine.ParentTransform = _gunTransform;
            magazine.AmmoSettings = _config.ammoSettings;
            return magazine;
        }

        private void InitializeMagazines()
        {
            for (int i = 0; i < _config.ammoSettings.magazineCount; i++)
            {
                var mag = _magazinePool.Get();
                mag.Capacity = _config.ammoSettings.magazineSize;
                mag.CurrentAmmo = _config.ammoSettings.magazineSize;
                _magazines.Add(mag);
                mag.UnEquip();
            }
            
            EquipCurrentMagazine();

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
        
        private void EquipCurrentMagazine()
        {
            CurrentMagazine.Equip();
        }

        private IEnumerator ReloadRoutine()
        {
            var currentMag = CurrentMagazine;
            currentMag.Drop();
            _magazines.RemoveAt(_currentMagazineIndex);
            
            yield return new WaitForSeconds(_config.ammoSettings.reloadTime);
            
            _currentMagazineIndex = (_currentMagazineIndex + 1) % _magazines.Count;
            _isReloading = false;
            
            VandullLogger.Log(this);
           EquipCurrentMagazine();
            
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

            return $"{CurrentMagazine.CurrentAmmo}/{_config.ammoSettings.magazineSize} | Magazines: {_magazines.Count}";
        }
    }
}