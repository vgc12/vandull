using Levels;
using Player.Input;
using Reflex.Attributes;
using UnityEngine;

namespace Items.Guns
{
    public sealed class ItemInputHandler : MonoBehaviour
    {
        [Inject] private readonly IPlayerInput _input;

        private Gun _currentGun;
        private Item _currentItem;
        private ItemHandler _itemHandler;


        public void Start()
        {
            _input.Aim += OnAim;

            _input.Reload += OnReload;

            _input.QuickReload += OnQuickReload;

            _input.CheckAmmo += OnCheckAmmo;

            _input.Attack += Use;

            _input.SwitchFireMode += OnFireModeSwitched;

            _input.Restart += OnRestart;

            _input.SwitchItem += OnItemSwitched;

            _itemHandler = GetComponent<ItemHandler>();
            _currentItem = _itemHandler.EquippedItem;
            if (_currentItem is Gun gun)
                _currentGun = gun;
        }

        private void OnDestroy()
        {
            _input.Aim -= OnAim;
            _input.Reload -= OnReload;
            _input.Attack -= Use;
            _input.SwitchFireMode -= OnFireModeSwitched;
            _input.Restart -= OnRestart;
            _input.SwitchItem -= OnItemSwitched;
            _input.CheckAmmo -= OnCheckAmmo;
            _input.QuickReload -= OnQuickReload;
        }

        private void OnCheckAmmo()
        {
            if (_currentGun == null) return;

            _currentGun.CheckAmmo();
        }

        private void OnItemSwitched(float value)
        {
            _itemHandler.SwitchItem((int)value);
            _currentItem = _itemHandler.EquippedItem;

            if (_currentItem is Gun newGun)
                _currentGun = newGun;
            else
                _currentGun = null;
        }


        private void OnRestart()
        {
            LevelManager.Instance.ReloadLevel();
        }

        private void Use((bool started, bool performed, bool canceled) context)
        {
            if (context.performed) _currentItem?.Use();
            if (context.canceled) _currentItem?.StopUse();
        }

        public void OnQuickReload()
        {
            if (_currentGun == null) return;

            _currentGun.StartReload(true);
        }

        public void OnReload()
        {
            if (_currentGun == null) return;

            _currentGun.StartReload(false);
        }


        private void OnFireModeSwitched()
        {
            if (!_currentGun) return;
            _currentGun.CycleFireMode();
        }


        public void OnAim(bool value)
        {
            if (_currentGun == null) return;
            if (value)
                _currentGun.StartAiming();
            else
                _currentGun.StopAiming();
        }
    }
}