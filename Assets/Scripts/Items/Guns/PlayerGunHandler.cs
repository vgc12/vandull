using EventBus;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items.Guns
{
    public class PlayerGunHandler : MonoBehaviour
    {
        private InputManager _inputManager;
        private Gun _currentGun;
        private bool _aimToggled;
        private EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;
        public void Start()
        {
            _inputManager = InputManager.Instance;
            _inputManager.InputActions.Player.Aim.started += OnAim;
            _itemSwitchedEventBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);
            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedEventBinding);

        }

        private void OnItemSwitched(ItemSwitchedEvent obj)
        {
            if(obj.NewItem is Gun newGun)
            {
                _currentGun?.StopAiming();
                _currentGun = newGun;
                if(_aimToggled)
                    _currentGun.StartAiming();
            }
            else
            {
                _currentGun?.StopAiming();
                _currentGun = null;
            }
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            
            if (context.started)
            {
                _aimToggled = !_aimToggled;
            }

            if (_aimToggled)
            {
                _currentGun.StartAiming();
            }
            else if (!_aimToggled)
            {
                _currentGun. StopAiming();
            }
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            if (context.started)
                _currentGun.StartReload();
        }

    }
}