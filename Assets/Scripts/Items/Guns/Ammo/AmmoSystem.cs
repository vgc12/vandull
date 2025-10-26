using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EventBus;
using General.Extensions;
using Player;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Items.Guns.Ammo
{
    public class AmmoSystem : IAmmoSystem
    {
        private readonly MonoBehaviour _behaviour;
        private readonly GunConfig _config;
        private readonly EventBinding<ItemSwitchedEvent> _itemSwitchedBinding;
        private readonly ObjectPool<Magazine> _magazinePool;
        private readonly List<Magazine> _magazines = new();
        private readonly Transform _magazineSpawnPosition;
        private readonly RigHandler _rigHandler;

        private bool _bulletInChamber = true;
        private int _currentMagazineIndex;
        private bool _weaponMustReload;

        public AmmoSystem(GunConfig config, Transform magazineSpawnPosition, RigHandler rigHandler)
        {
            _config = config;
            _behaviour = rigHandler;
            _rigHandler = rigHandler;
            _magazineSpawnPosition = magazineSpawnPosition;

            _magazinePool = new ObjectPool<Magazine>(
                CreateMagazine,
                null,
                mag => mag.UnEquip(),
                mag => Object.Destroy(mag.gameObject)
            );

            _itemSwitchedBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);
            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedBinding);

            InitializeMagazines();
        }

        // Properties
        private Magazine CurrentMagazine { get; set; }
        private bool HasSpareAmmo => _magazines.Count > 1;

        public bool CurrentMagazineEmpty => CurrentMagazine.IsEmpty;
        public bool IsReloading { get; private set; }
        public int CurrentAmmo => CurrentMagazine.CurrentAmmo;
        public int TotalAmmo => _magazines.Count * _config.ammoSettings.magazineSize;
        public bool OutOfAmmo => CurrentMagazine.IsEmpty && !_bulletInChamber;
        public bool CanReload => !IsReloading && HasSpareAmmo;

        // Events
        public Action<ReloadEvent> OnReloadComplete { get; set; }
        public Action OnOutOfAmmo { get; set; }

        // Public Methods
        public void StartReload()
        {
            if (!CanReload) return;

            IsReloading = true;
            _behaviour.StartCoroutine(ReloadRoutine());
        }

        public void ConsumeAmmo()
        {
            if (!CurrentMagazineEmpty)
                CurrentMagazine.ConsumeAmmo();
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

        public void DropMagazine()
        {
            var magazineToDrop = CurrentMagazine;
            magazineToDrop.Drop();

            if (_currentMagazineIndex >= 0 && _currentMagazineIndex < _magazines.Count)
                _magazines.RemoveAt(_currentMagazineIndex);
        }

        ~AmmoSystem()
        {
            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedBinding);
        }

        // Magazine Management
        private void InitializeMagazines()
        {
            var ammoSettings = _config.ammoSettings;

            for (var i = 0; i < ammoSettings.magazineCount; i++)
            {
                var magazine = _magazinePool.Get();
                magazine.Initialize(ammoSettings, _magazineSpawnPosition);
                magazine.UnEquip();
                _magazines.Add(magazine);
            }

            EquipCurrentMagazine();
        }

        private void EquipCurrentMagazine()
        {
            CurrentMagazine = _magazines[_currentMagazineIndex];
            CurrentMagazine.Equip();
        }

        // Reload Logic
        private IEnumerator ReloadRoutine()
        {
            // Disable rig syncing during reload
            _rigHandler.FollowItemTargets = false;

            // Drop current magazine if still equipped
            if (!CurrentMagazine.IsDropped) DropMagazine();

            // Wait for reload animation
            yield return new WaitForSeconds(_config.ammoSettings.reloadTime);

            // Switch to next magazine (wraps around)
            _currentMagazineIndex = (_currentMagazineIndex + 1) % _magazines.Count;
            IsReloading = false;

            EquipCurrentMagazine();

            // Chamber a round if needed
            if (!_bulletInChamber)
            {
                _bulletInChamber = true;
                CurrentMagazine.ConsumeAmmo();
            }

            OnReloadComplete?.Invoke(new ReloadEvent(CurrentMagazine));
        }

        // Event Handlers
        private void OnItemSwitched(ItemSwitchedEvent evt)
        {
            if (!IsReloading) return;
            if (evt.NewItem.transform.root.gameObject.layer != LayerMask.NameToLayer("Player")) return;

            // Cancel current reload and mark for retry
            IsReloading = false;
            _behaviour?.StopAllCoroutines();
            _weaponMustReload = true;
        }

        // Factory
        private Magazine CreateMagazine()
        {
            var magazineObject = Object.Instantiate(_config.ammoSettings.magazinePrefab);

            // Ensure required components
            magazineObject.GetOrAdd<Rigidbody>();
            magazineObject.GetOrAdd<BoxCollider>();

            return magazineObject.GetOrAdd<Magazine>();
        }

        // Debug/Display
        public override string ToString()
        {
            return GetAllMagsStatus();
        }

        public string GetAllMagsStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetCurrentMagazineStatus());

            for (var i = 0; i < _magazines.Count; i++) sb.AppendLine(GetMagazineStatus(i));

            return sb.ToString();
        }

        private string GetCurrentMagazineStatus()
        {
            return $"{CurrentMagazine.CurrentAmmo}/{_config.ammoSettings.magazineSize} | Magazines: {_magazines.Count}";
        }

        private string GetMagazineStatus(int index)
        {
            if (index < 0 || index >= _magazines.Count) return string.Empty;

            var mag = _magazines[index];
            return $"Magazine {index + 1}: {mag.CurrentAmmo}/{_config.ammoSettings.magazineSize}";
        }
    }
}