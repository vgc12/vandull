using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EventBus;
using General;
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
        private readonly Gun _gun;
        private readonly EventBinding<ItemSwitchedEvent> _itemSwitchedBinding;
        private readonly ObjectPool<Magazine> _magazinePool;
        private readonly List<Magazine> _magazines = new();
        private readonly Transform _magazineSpawnPosition;

        private readonly StopwatchTimer _reloadTimer;
        private readonly RigHandler _rigHandler;

        private bool _bulletInChamber = true;
        private int _currentMagazineIndex;

        private ItemAnimation _interruptedReloadAnimation;
        private float _interruptedReloadTime;

        private Coroutine _reloadCoroutine;
        private QueuedReloadType _reloadQueued;

        public AmmoSystem(Gun gun, RigHandler rigHandler)
        {
            _gun = gun;
            _behaviour = rigHandler;
            _rigHandler = rigHandler;
            _magazineSpawnPosition = gun.magazinePosition;


            /*_reloadTimer = new StopwatchTimer();
            _reloadTimer.OnTimerStop += OnReloadTimerStop;*/

            _magazinePool = new ObjectPool<Magazine>(
                CreateMagazine,
                null,
                mag => mag.UnEquip(),
                mag => Object.Destroy(mag.gameObject)
            );

            /*
            _itemSwitchedBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);
           EventBus<ItemSwitchedEvent>.Register(_itemSwitchedBinding);
           */

            InitializeMagazines();
        }

        private bool HasSpareAmmo => _magazines.Count > 1;

        public bool CurrentMagazineEmpty => !CurrentMagazine || CurrentMagazine.IsEmpty;

        // Properties
        public Magazine CurrentMagazine { get; private set; }
        public bool IsReloading { get; private set; }
        public int CurrentAmmo => CurrentMagazine.CurrentAmmo;
        public int TotalAmmo => _magazines.Count * _gun.ammoSettings.magazineSize;
        public bool OutOfAmmo => !CurrentMagazine || (CurrentMagazine.IsEmpty && !_bulletInChamber);
        public bool CanReload => !IsReloading && HasSpareAmmo;

        // Events
        public Action<ReloadEvent> OnReloadComplete { get; set; }
        public Action OnOutOfAmmo { get; set; }


        // Public Methods
        public void StartReload()
        {
            if (!CanReload || _reloadCoroutine != null) return;

            _reloadQueued = QueuedReloadType.Normal;
            IsReloading = true;
            var length =
                _gun.ItemAnimationSystem.PlayAnimationAndGetLength(_gun.reloadAnimation, _interruptedReloadTime);
            _reloadCoroutine = _behaviour.StartCoroutine(ReloadRoutine(length));
        }

        public void StartQuickReload()
        {
            if (!CanReload || _reloadCoroutine != null) return;

            _reloadQueued = QueuedReloadType.Quick;
            IsReloading = true;
            var length =
                _gun.ItemAnimationSystem.PlayAnimationAndGetLength(_gun.quickReloadAnimation, _interruptedReloadTime);
            _reloadCoroutine = _behaviour.StartCoroutine(ReloadRoutine(length));
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
            /*
            _reloadTimer.Tick(Time.deltaTime);
            if (_reloadQueued == QueuedReloadType.None || !_gun.IsEquipped || _reloadCoroutine != null) return;

            switch (_reloadQueued)
            {
                case QueuedReloadType.Normal:
                    StartReload();

                    break;
                case QueuedReloadType.Quick:
                    StartQuickReload();
                    break;
            }
            */
        }

        public void DropMagazine()
        {
            var magazineToDrop = CurrentMagazine;
            if (magazineToDrop != null) magazineToDrop.Drop();

            if (_currentMagazineIndex >= 0 && _currentMagazineIndex < _magazines.Count)
                _magazines.RemoveAt(_currentMagazineIndex);
            CurrentMagazine = null;
        }

        public void EquipNewMagazine()
        {
            _currentMagazineIndex = (_currentMagazineIndex + 1) % _magazines.Count;
            EquipCurrentMagazine();
        }

        public void RemoveCurrentMagazine()
        {
            if (!CurrentMagazine) return;
            CurrentMagazine.UnEquip();
            CurrentMagazine = null;
        }

        private void OnReloadTimerStop()
        {
            _interruptedReloadTime = _reloadTimer.GetTime();
        }


        ~AmmoSystem()
        {
            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedBinding);
        }

        // Magazine Management
        private void InitializeMagazines()
        {
            var ammoSettings = _gun.ammoSettings;

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
        private IEnumerator ReloadRoutine(float length)
        {
            // Disable rig syncing during reload

            _reloadTimer.Start();
            _rigHandler.LeftHandFollowItemTarget = false;

            var seconds = new WaitForSeconds(length - _interruptedReloadTime);

            // Wait for reload animation
            yield return seconds;

            _reloadTimer.Stop();
            if (_gun.Owner == OwnerStatus.Enemy) EquipNewMagazine();
            IsReloading = false;


            // Chamber a round if needed
            if (!_bulletInChamber)
            {
                _bulletInChamber = true;
                CurrentMagazine.ConsumeAmmo();
            }

            _rigHandler.LeftHandFollowItemTarget = true;
            _gun.ItemAnimationSystem.PlayAnimation(_gun.holdingItemAnimation);
            _reloadQueued = QueuedReloadType.None;
            _reloadCoroutine = null;
            _interruptedReloadTime = 0;
            OnReloadComplete?.Invoke(new ReloadEvent(CurrentMagazine));
        }


        // Event Handlers
        private void OnItemSwitched(ItemSwitchedEvent evt)
        {
            if (!IsReloading || _gun.Owner == OwnerStatus.Enemy) return;

            _reloadTimer.Stop();
            _reloadTimer.Reset();
            // Cancel current reload and mark for retry
            IsReloading = false;

            if (_reloadCoroutine != null)
            {
                _behaviour.StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = null;
            }

            _rigHandler.LeftHandFollowItemTarget = true;
        }

        // Factory
        private Magazine CreateMagazine()
        {
            var magazineObject = Object.Instantiate(_gun.ammoSettings.magazinePrefab);

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
            return $"{CurrentMagazine.CurrentAmmo}/{_gun.ammoSettings.magazineSize} | Magazines: {_magazines.Count}";
        }

        private string GetMagazineStatus(int index)
        {
            if (index < 0 || index >= _magazines.Count) return string.Empty;

            var mag = _magazines[index];
            return $"Magazine {index + 1}: {mag.CurrentAmmo}/{_gun.ammoSettings.magazineSize}";
        }

        private enum QueuedReloadType
        {
            None,
            Normal,
            Quick
        }
    }
}