using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EventBus;
using General;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Items.Guns.Ammo
{
    public class AmmoSystem : IAmmoSystem
    {
        private readonly MonoBehaviour _behaviour;

        private readonly GunConfig _config;

        private readonly EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;

        private readonly ObjectPool<Magazine> _magazinePool;

        private readonly GameObject _magazinePrefab;
        private readonly List<Magazine> _magazines = new();


        private readonly Transform _magazineSpawnPosition;


        private bool _bulletInChamber;
        private int _currentMagazineIndex;

        private bool _weaponMustReload;

        public AmmoSystem(GunConfig config, Transform magazineSpawnPosition, MonoBehaviour behaviour)
        {
            _config = config;

            _behaviour = behaviour;
            _magazinePrefab = config.ammoSettings.magazinePrefab;
            _magazinePool = new ObjectPool<Magazine>(CreateMagazine);
            _magazineSpawnPosition = magazineSpawnPosition;
            _bulletInChamber = true;
            _itemSwitchedEventBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);
            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedEventBinding);
            InitializeMagazines();
        }

        private Magazine CurrentMagazine { get; set; }

        private bool HasSpareAmmo => _magazines.Count > 1;
        public bool CurrentMagazineEmpty => CurrentMagazine.IsEmpty;
        public bool IsReloading { get; private set; }

        public int CurrentAmmo => CurrentMagazine.CurrentAmmo;
        public int TotalAmmo => _magazines.Count * _config.ammoSettings.magazineSize;
        public bool OutOfAmmo => CurrentMagazine.IsEmpty && !_bulletInChamber;
        public event Action<ReloadEvent> OnReloadComplete;
        public event Action OnOutOfAmmo;

        public bool CanReload => !IsReloading && HasSpareAmmo;

        public void StartReload()
        {
            if (!CanReload) return;

            IsReloading = true;

            _behaviour.StartCoroutine(ReloadRoutine());
        }


        public void DropMagazine()
        {
            var currentMag = CurrentMagazine;
            currentMag.Drop();
            if (_currentMagazineIndex >= _magazines.Count || _currentMagazineIndex < 0) return;
            _magazines.RemoveAt(_currentMagazineIndex);
        }


        public void ConsumeAmmo()
        {
            if (!CurrentMagazineEmpty)
                CurrentMagazine.SubtractOne();
            else if (_bulletInChamber)
                _bulletInChamber = false;
            else
                OnOutOfAmmo?.Invoke();
        }

        public void Update()
        {
            if (!_weaponMustReload) return;
            _weaponMustReload = false;
            _behaviour.StartCoroutine(ReloadRoutine());
        }

        private Magazine CreateMagazine()
        {
            var magObject = Object.Instantiate(_magazinePrefab);

            magObject.GetOrAdd<Rigidbody>();
            magObject.GetOrAdd<BoxCollider>();
            magObject.GetOrAdd<MeshRenderer>();

            var magazine = magObject.GetOrAdd<Magazine>();
            magazine.MagazinePosition = _magazineSpawnPosition;
            magazine.AmmoSettings = _config.ammoSettings;
            return magazine;
        }

        private void InitializeMagazines()
        {
            for (var i = 0; i < _config.ammoSettings.magazineCount; i++)
            {
                var mag = _magazinePool.Get();
                mag.Capacity = _config.ammoSettings.magazineSize;
                mag.CurrentAmmo = _config.ammoSettings.magazineSize;
                _magazines.Add(mag);
                mag.UnEquip();
            }

            EquipCurrentMagazine();
        }

        private void EquipCurrentMagazine()
        {
            CurrentMagazine = _magazines[_currentMagazineIndex];
            CurrentMagazine.Equip();
        }

        private IEnumerator ReloadRoutine()
        {
            if (!CurrentMagazine.IsDropped) DropMagazine();

            yield return new WaitForSeconds(_config.ammoSettings.reloadTime);

            _currentMagazineIndex = (_currentMagazineIndex + 1) % _magazines.Count;
            IsReloading = false;


            EquipCurrentMagazine();

            if (!_bulletInChamber)
            {
                _bulletInChamber = true;
                CurrentMagazine.SubtractOne();
            }

            OnReloadComplete?.Invoke(new ReloadEvent(CurrentMagazine));
        }

        public void OnItemSwitched(ItemSwitchedEvent evt)
        {
            if (IsReloading && evt.NewItem.transform.root.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                IsReloading = false;
                _behaviour?.StopAllCoroutines();
                _weaponMustReload = true;
            }
        }


        public override string ToString()
        {
            return GetAllMagsStatus();
        }

        public string GetAllMagsStatus()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(MagazineStatus());

            for (var i = 0; i < _magazines.Count; i++) stringBuilder.AppendLine(MagazineStatus(i));

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