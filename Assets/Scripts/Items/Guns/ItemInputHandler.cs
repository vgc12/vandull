using System;
using Attributes;
using EventBus;
using General;
using Items.Guns.Recoil;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Items.Guns
{
    public class ItemInputHandler : MonoBehaviour
    {
        private InputManager _inputManager;
        private Item _currentItem;
        private Gun _currentGun;
        private bool _aimToggled;
        private EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;

        private void Awake()
        {
            _itemSwitchedEventBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);
         
            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedEventBinding);
        }

        public void Start()
        {
            _inputManager = InputManager.Instance;
            _inputManager.InputActions.Player.Aim.started += OnAim;
            _inputManager.InputActions.Player.Aim.canceled += OnAim;
            _inputManager.InputActions.Player.Reload.started += OnReload;
            _inputManager.InputActions.Player.Reload.canceled += OnReload;
    
            _inputManager.InputActions.Player.Attack.started += Use;
            _inputManager.InputActions.Player.Attack.performed += Use;
            _inputManager.InputActions.Player.Attack.canceled += Use;
            
            _inputManager.InputActions.Player.SwitchFireMode.started += OnFireModeSwitched;
            _inputManager.InputActions.Player.SwitchFireMode.canceled += OnFireModeSwitched;

            _inputManager.InputActions.Player.Restart.performed += OnRestart;

        }

        public void OnRestart(InputAction.CallbackContext obj)
        {
            if(obj.performed)
                SceneManager.LoadScene( SceneManager.GetActiveScene().name );
        }

        private void OnFireModeSwitched(InputAction.CallbackContext obj)
        {
            if(!_currentGun) return;
            if (obj.started)
            {
                _currentGun.CycleFireMode();
            }
        }
        

        private void Use(InputAction.CallbackContext obj)
        {
            if(_currentItem == null) return;
            _currentItem.Use(obj);
        }

        private void OnItemSwitched(ItemSwitchedEvent obj)
        {
            _currentItem = obj.NewItem;
            if(obj.NewItem is Gun newGun)
            {
                if (_currentGun != null)
                {
                    _currentGun.StopAiming();
                }
                _currentGun = newGun;
           
            }
            else
            {
                _currentGun = null;
            }
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if(_currentGun == null) return;
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
            if(_currentItem == null) return;
            if (context.started)
                _currentGun.StartReload();
        }

        private void OnDestroy()
        {
            _inputManager.InputActions.Player.Aim.started -= OnAim;
            _inputManager.InputActions.Player.Aim.canceled -= OnAim;
            _inputManager.InputActions.Player.Reload.started -= OnReload;
            _inputManager.InputActions.Player.Reload.canceled -= OnReload;
            _inputManager.InputActions.Player.Attack.started -= Use;
            _inputManager.InputActions.Player.Attack.performed -= Use;
            _inputManager.InputActions.Player.Attack.canceled -= Use;
            _inputManager.InputActions.Player.SwitchFireMode.started -= OnFireModeSwitched;
            _inputManager.InputActions.Player.SwitchFireMode.canceled -= OnFireModeSwitched;
            _inputManager.InputActions.Player.Restart.performed -= OnRestart;
            
            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedEventBinding);
        }
    }
}