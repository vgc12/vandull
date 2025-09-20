using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EventBus;
using General;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Items.Guns
{
    

    
    
    
    
    public class AmmoSystem : IAmmoSystem
    {
        
        private readonly GunConfig _config;
        private readonly List<Magazine> _magazines = new();
        private int _currentMagazineIndex;
        private bool _isReloading;
        private readonly MonoBehaviour _behaviour;
        public bool IsCurrentMagazineEmpty => CurrentMagazine.IsEmpty;
        public bool IsReloading => _isReloading;
        public int CurrentAmmo => CurrentMagazine.CurrentAmmo;
        public int TotalAmmo => _magazines.Count * _config.ammoSettings.magazineSize;

        private Magazine CurrentMagazine => _magazines[_currentMagazineIndex];
        
        private readonly ObjectPool<Magazine> _magazinePool;

        private readonly GameObject _magazinePrefab;
        
        private readonly Transform _gunTransform;
        
        public AmmoSystem(Gun gun)
        {
            _config = gun.gunConfig;
            _gunTransform = gun.transform;
            _behaviour = gun;
            _magazinePrefab = gun.magazinePrefab;
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
            
           EventBus<ReloadEvent>.Raise(new ReloadEvent(CurrentMagazine));
    
        }

        public class ReloadEvent : IEvent
        {
            public Magazine CurrentMagazine { get; }
            
            public ReloadEvent(Magazine currentMagazine)
            {
                CurrentMagazine = currentMagazine;
            }
            
        }

      

        public void ConsumeAmmo()
        {
            CurrentMagazine.SubtractOne();
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