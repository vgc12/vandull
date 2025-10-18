using EventBus;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Items.Guns
{
    public class ItemInputHandler : MonoBehaviour
    {
        private bool _aimToggled;
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
            InputManager.Instance.InputActions.Player.Aim.started += OnAim;
            InputManager.Instance.InputActions.Player.Aim.canceled += OnAim;
            InputManager.Instance.InputActions.Player.Reload.started += OnReload;
            InputManager.Instance.InputActions.Player.Reload.canceled += OnReload;

            InputManager.Instance.InputActions.Player.Attack.started += Use;
            InputManager.Instance.InputActions.Player.Attack.performed += Use;
            InputManager.Instance.InputActions.Player.Attack.canceled += Use;

            InputManager.Instance.InputActions.Player.SwitchFireMode.started += OnFireModeSwitched;
            InputManager.Instance.InputActions.Player.SwitchFireMode.canceled += OnFireModeSwitched;

            InputManager.Instance.InputActions.Player.Restart.performed += OnRestart;
        }

        private void OnDestroy()
        {
            InputManager.Instance.InputActions.Player.Aim.started -= OnAim;
            InputManager.Instance.InputActions.Player.Aim.canceled -= OnAim;
            InputManager.Instance.InputActions.Player.Reload.started -= OnReload;
            InputManager.Instance.InputActions.Player.Reload.canceled -= OnReload;
            InputManager.Instance.InputActions.Player.Attack.started -= Use;
            InputManager.Instance.InputActions.Player.Attack.performed -= Use;
            InputManager.Instance.InputActions.Player.Attack.canceled -= Use;
            InputManager.Instance.InputActions.Player.SwitchFireMode.started -= OnFireModeSwitched;
            InputManager.Instance.InputActions.Player.SwitchFireMode.canceled -= OnFireModeSwitched;
            InputManager.Instance.InputActions.Player.Restart.performed -= OnRestart;

            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedEventBinding);
        }

        public void OnRestart(InputAction.CallbackContext obj)
        {
            if (obj.performed)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnFireModeSwitched(InputAction.CallbackContext obj)
        {
            if (!_currentGun) return;
            if (obj.started) _currentGun.CycleFireMode();
        }


        private void Use(InputAction.CallbackContext obj)
        {
            if (_currentItem is not Gun gun) return;
            if (obj.started)
                gun.ExecuteSingleShot();
            else if (obj.performed)
                gun.StartAutomaticFire();
            else if (obj.canceled) gun.StopAutomaticFire();
        }

        private void OnItemSwitched(ItemSwitchedEvent obj)
        {
            _currentItem = obj.NewItem;
            // Important that this gets toggled off, when item is switched
            _aimToggled = false;
            if (obj.NewItem is Gun newGun)
            {
                if (_currentGun != null) _currentGun.StopAiming();
                _currentGun = newGun;
            }
            else
            {
                _currentGun = null;
            }
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (_currentGun == null) return;
            if (context.started) _aimToggled = !_aimToggled;

            if (_aimToggled)
                _currentGun.StartAiming();
            else if (!_aimToggled) _currentGun.StopAiming();
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            if (_currentItem == null) return;
            if (context.started)
                _currentGun.StartReload();
        }
    }
}