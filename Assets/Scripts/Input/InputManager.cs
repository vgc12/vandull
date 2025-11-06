using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EventBus;
using Reflex.Attributes;
using UI.States;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;
using ILogger = General.Logging.ILogger;

namespace Player.Input
{
    public class InputManager : IInputService, IPlayerActions, IUIActions
    {
        private readonly EventBinding<SettingsUIState.ControlSettingsChangedEvent> _controlsChangedEventBinding;
        private readonly float _crouchInputBuffer = 0.002f;
        private readonly float _doubleTapWindow = 0.3f;
        private readonly EventBinding<UIStateSwitchedEvent> _uiStateChangedEventBinding;

        private bool _aimToggled;
        private bool _crouchToggled;
        private SettingsUIState.ControlSettingsChangedEvent _currentControlSettings;
        private float _lastAimInputTime;
        private float _lastCrouchInputTime;

        // Double-tap reload fields
        private float _lastReloadTapTime;
        private float _lastSprintInputTime;

        [Inject] private ILogger _logger;
        private CancellationTokenSource _reloadCts;

        private bool _sprintToggled;
        private UIStateType _uiState;

        public InputManager()
        {
            var guid = Guid.NewGuid().ToString();
            Debug.Log($"[InputManager] Created InputManager with GUID: {guid}");
            Initialize();
            EnablePlayerActions();
            _uiStateChangedEventBinding = new EventBinding<UIStateSwitchedEvent>(OnUIStateChanged);
            EventBus<UIStateSwitchedEvent>.Register(_uiStateChangedEventBinding);
            _controlsChangedEventBinding =
                new EventBinding<SettingsUIState.ControlSettingsChangedEvent>(OnControlsChanged);
            EventBus<SettingsUIState.ControlSettingsChangedEvent>.Register(_controlsChangedEventBinding);
            
        }

        public PlayerInputActions InputActions { get; private set; }

        // IInputReader properties
        public Vector2 Direction => InputActions.Player.Move.ReadValue<Vector2>();

        // Player action events
        public event UnityAction<(bool started, bool performed, bool canceled)> Attack = delegate { };
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2> Look = delegate { };
        public event UnityAction Interact = delegate { };
        public event UnityAction<bool> Crouch = delegate { };
        public event UnityAction<bool> Jump = delegate { };
        public event UnityAction Previous = delegate { };
        public event UnityAction Next = delegate { };
        public event UnityAction<bool> Sprint = delegate { };
        public event UnityAction<float> Lean = delegate { };
        public event UnityAction<bool> Aim = delegate { };
        public event UnityAction<float> SwitchItem = delegate { };
        public event UnityAction Reload = delegate { };
        public event UnityAction QuickReload = delegate { }; // New event for double-tap
        public event UnityAction SwitchFireMode = delegate { };
        public event UnityAction Restart = delegate { };

        // UI events
        public event UnityAction InGameCancel = delegate { };
        public event UnityAction UIDisengaged = delegate { };
        public event UnityAction<Vector2> Navigate = delegate { };
        public event UnityAction Submit = delegate { };
        public event UnityAction InMenuCancel = delegate { };
        public event UnityAction<Vector2> Point = delegate { };
        public event UnityAction Click = delegate { };
        public event UnityAction RightClick = delegate { };
        public event UnityAction MiddleClick = delegate { };
        public event UnityAction<Vector2> ScrollWheel = delegate { };
        public event UnityAction<Vector3> TrackedDevicePosition = delegate { };
        public event UnityAction<Quaternion> TrackedDeviceOrientation = delegate { };

        public void Initialize()
        {
            if (InputActions == null)
            {
                InputActions = new PlayerInputActions();
                InputActions.Player.SetCallbacks(this);
                InputActions.UI.SetCallbacks(this);
                _currentControlSettings = new SettingsUIState.ControlSettingsChangedEvent();
            }
        }

        public void Cleanup()
        {
            _reloadCts?.Cancel();
            _reloadCts?.Dispose();
            _reloadCts = null;
        }

        // Player Actions
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled) Move.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled) Look.Invoke(context.ReadValue<Vector2>());
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            Attack.Invoke((context.started, context.performed, context.canceled));
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed) Interact.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (_currentControlSettings.ToggleCrouch && context.started &&
                Time.time - _lastCrouchInputTime > _crouchInputBuffer)
            {
                _lastCrouchInputTime = Time.time;
                _crouchToggled = !_crouchToggled;
                Crouch.Invoke(_crouchToggled);
                return;
            }

            if (_currentControlSettings.ToggleCrouch) return;

            if (context.performed)
                Crouch.Invoke(true);
            else if (context.canceled)
                Crouch.Invoke(false);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started || context.canceled) Jump.Invoke(context.started);
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed) Previous.Invoke();
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (context.performed) Next.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (_currentControlSettings.ToggleSprint && context.started &&
                Time.time - _lastSprintInputTime > _crouchInputBuffer)
            {
                _lastSprintInputTime = Time.time;
                _sprintToggled = !_sprintToggled;
                Sprint.Invoke(_sprintToggled);
                return;
            }

            if (_currentControlSettings.ToggleSprint) return;
            if (context.performed)
                Sprint.Invoke(true);
            else if (context.canceled)
                Sprint.Invoke(false);
        }

        public void OnLean(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled)
                Lean.Invoke(context.ReadValue<float>());
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (_currentControlSettings.ToggleAim && context.started &&
                Time.time - _lastAimInputTime > _crouchInputBuffer)
            {
                _lastAimInputTime = Time.time;
                _aimToggled = !_aimToggled;
                Aim.Invoke(_aimToggled);
                return;
            }

            if (_currentControlSettings.ToggleAim) return;
            if (context.performed)
                Aim.Invoke(true);
            else if (context.canceled)
                Aim.Invoke(false);
        }

        public void OnSwitchItem(InputAction.CallbackContext context)
        {
            if (context.performed) SwitchItem.Invoke(context.ReadValue<float>());
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            if (!context.started) return;

            var timeSinceLastTap = Time.time - _lastReloadTapTime;

            if (timeSinceLastTap <= _doubleTapWindow)
            {
                // Double tap detected - cancel pending normal reload
                _reloadCts?.Cancel();
                _reloadCts?.Dispose();
                _reloadCts = null;

                QuickReload.Invoke();
                _lastReloadTapTime = 0; // Reset to prevent triple-tap issues
            }
            else
            {
                // Start delayed normal reload
                _reloadCts?.Cancel();
                _reloadCts?.Dispose();
                _reloadCts = new CancellationTokenSource();

                DelayedReload(_reloadCts.Token).Forget();
                _lastReloadTapTime = Time.time;
            }
        }

        public void OnSwitchFireMode(InputAction.CallbackContext context)
        {
            if (context.started) SwitchFireMode.Invoke();
        }

        public void OnRestart(InputAction.CallbackContext context)
        {
            if (context.performed) Restart.Invoke();
        }

        public void OnUIEngage(InputAction.CallbackContext context)
        {
            if (_uiState != UIStateType.InGame) return;
            InGameCancel.Invoke();
            EnableUIActions();
        }

        // UI Actions
        public void OnNavigate(InputAction.CallbackContext context)
        {
            if (context.performed) Navigate.Invoke(context.ReadValue<Vector2>());
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.performed) Submit.Invoke();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            InMenuCancel.Invoke();
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled) Point.Invoke(context.ReadValue<Vector2>());
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.performed) Click.Invoke();
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (context.performed) RightClick.Invoke();
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
            if (context.performed) MiddleClick.Invoke();
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
            if (context.performed) ScrollWheel.Invoke(context.ReadValue<Vector2>());
        }

        public void OnTrackedDevicePosition(InputAction.CallbackContext context)
        {
            if (context.performed) TrackedDevicePosition.Invoke(context.ReadValue<Vector3>());
        }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
        {
            if (context.performed) TrackedDeviceOrientation.Invoke(context.ReadValue<Quaternion>());
        }

        private async UniTaskVoid DelayedReload(CancellationToken ct)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_doubleTapWindow), cancellationToken: ct);

                if (!ct.IsCancellationRequested) Reload.Invoke();
            }
            catch (OperationCanceledException)
            {
                // Expected when double tap occurs - do nothing
            }
        }

        private void OnControlsChanged(SettingsUIState.ControlSettingsChangedEvent obj)
        {
            _currentControlSettings = obj;
        }

        public void EnableUIActions()
        {
            InputActions.UI.Enable();
            SetPlayerActionsEnabled(false);
        }

        public void EnablePlayerActions()
        {
            InputActions.UI.Disable();
            SetPlayerActionsEnabled(true);
        }

        private void OnDestroy()
        {
            EventBus<UIStateSwitchedEvent>.Deregister(_uiStateChangedEventBinding);
            InputActions?.Disable();

            // Clean up cancellation token
            _reloadCts?.Cancel();
            _reloadCts?.Dispose();
            _reloadCts = null;
        }

        private void OnUIStateChanged(UIStateSwitchedEvent evt)
        {
            _uiState = evt.NewState;
            if (_uiState == UIStateType.InGame)
                EnablePlayerActions();
            else
                EnableUIActions();
        }

        private void SetPlayerActionsEnabled(bool value)
        {
            var map = InputActions.asset.FindActionMap("Player");

            foreach (var action in map.actions)
                if (value)
                    action.Enable();
                else
                    action.Disable();
        }

        private static bool IsMouse(InputAction.CallbackContext context)
        {
            return context.control.device is Mouse;
        }
    }
}