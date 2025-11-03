using EventBus;
using Levels;
using Player.Input;
using Reflex.Attributes;
using UnityEngine;

namespace Items.Guns
{
    public class ItemInputHandler : MonoBehaviour
    {
        [Inject] private readonly IPlayerInput _input;

        private Gun _currentGun;
        private Item _currentItem;

        private EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;

        private void Awake()
        {
            _itemSwitchedEventBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);

            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedEventBinding);
        }

        public void Start()
        {
            _input.Aim += OnAim;

            _input.Reload += OnReload;

            _input.QuickReload += OnQuickReload;


            _input.Attack += Use;

            _input.SwitchFireMode += OnFireModeSwitched;

            _input.Restart += OnRestart;
        }

        private void OnDestroy()
        {
            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedEventBinding);
            _input.Aim -= OnAim;
            _input.Reload -= OnReload;
            _input.Attack -= Use;
            _input.SwitchFireMode -= OnFireModeSwitched;
            _input.Restart -= OnRestart;
        }


        private void OnRestart()
        {
            LevelManager.Instance.ReloadLevel();
        }

        private void Use((bool started, bool performed, bool canceled) context)
        {
            _currentItem?.Use();
        }

        public void OnQuickReload()
        {
            if (_currentItem == null) return;

            _currentGun.StartReload(true);
        }

        public void OnReload()
        {
            if (_currentItem == null) return;

            _currentGun.StartReload(false);
        }


        private void OnFireModeSwitched()
        {
            if (!_currentGun) return;
            _currentGun.CycleFireMode();
        }

        private void OnItemSwitched(ItemSwitchedEvent obj)
        {
            if (!obj.NewItem || obj.NewItem.Owner != OwnerStatus.Player) return;
            _currentItem = obj.NewItem;
            // Important that this gets toggled off, when item is switched

            if (obj.NewItem is Gun newGun)
                _currentGun = newGun;
            else
                _currentGun = null;
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