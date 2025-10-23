using EventBus;
using UI.States;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;

namespace Player
{
    public interface IInputManager
    {
        bool IsUIEngaged { get; }
        void EnablePlayerActions();
        void Cleanup();
    }

    // Player input events
    public interface IPlayerInput
    {
        Vector2 Direction { get; }
        event UnityAction Attack;
        event UnityAction<Vector2> Move;
        event UnityAction<Vector2> Look;
        event UnityAction Interact;
        event UnityAction<bool> Crouch;
        event UnityAction<bool> Jump;
        event UnityAction Previous;
        event UnityAction Next;
        event UnityAction<bool> Sprint;
        event UnityAction<float> Lean;
        event UnityAction<bool> Aim;
        event UnityAction<float> SwitchItem;
        event UnityAction Reload;
        event UnityAction SwitchFireMode;
        event UnityAction Restart;
    }

    // UI input events
    public interface IUIInput
    {
        event UnityAction UIEngaged;
        event UnityAction UIDisengaged;
        event UnityAction<Vector2> Navigate;
        event UnityAction Submit;
        event UnityAction Cancel;
        event UnityAction<Vector2> Point;
        event UnityAction Click;
        event UnityAction RightClick;
        event UnityAction MiddleClick;
        event UnityAction<Vector2> ScrollWheel;
        event UnityAction<Vector3> TrackedDevicePosition;
        event UnityAction<Quaternion> TrackedDeviceOrientation;
    }

    // Complete interface
    public interface IInputService : IInputManager, IPlayerInput, IUIInput
    {
    }

    public class InputManager : IInputService, IInputReader, IPlayerActions, IUIActions
    {
        private readonly EventBinding<UIStateSwitchedEvent> _uiStateChangedEventBinding;
        private UIStateType _uiState;

        public InputManager()
        {
            _uiStateChangedEventBinding = new EventBinding<UIStateSwitchedEvent>(OnUIStateChanged);
            EventBus<UIStateSwitchedEvent>.Register(_uiStateChangedEventBinding);
        }

        public PlayerInputActions InputActions { get; private set; }
        public bool IsUIEngaged { get; private set; }

        // IInputReader properties
        public Vector2 Direction => InputActions.Player.Move.ReadValue<Vector2>();

        // Player action events
        public event UnityAction Attack = delegate { };
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
        public event UnityAction SwitchFireMode = delegate { };
        public event UnityAction Restart = delegate { };

        // UI events
        public event UnityAction UIEngaged = delegate { };
        public event UnityAction UIDisengaged = delegate { };
        public event UnityAction<Vector2> Navigate = delegate { };
        public event UnityAction Submit = delegate { };
        public event UnityAction Cancel = delegate { };
        public event UnityAction<Vector2> Point = delegate { };
        public event UnityAction Click = delegate { };
        public event UnityAction RightClick = delegate { };
        public event UnityAction MiddleClick = delegate { };
        public event UnityAction<Vector2> ScrollWheel = delegate { };
        public event UnityAction<Vector3> TrackedDevicePosition = delegate { };
        public event UnityAction<Quaternion> TrackedDeviceOrientation = delegate { };

        public void EnablePlayerActions()
        {
            if (InputActions == null)
            {
                InputActions = new PlayerInputActions();
                InputActions.Player.SetCallbacks(this);
                InputActions.UI.SetCallbacks(this);
            }

            InputActions.Enable();
            InputActions.UI.Disable();
            IsUIEngaged = false;
        }

        public void Cleanup()
        {
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
            if (context.performed && !IsUIEngaged) Attack.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed && !IsUIEngaged) Interact.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (!IsUIEngaged)
            {
                if (context.performed)
                    Crouch.Invoke(true);
                else if (context.canceled)
                    Crouch.Invoke(false);
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started && !IsUIEngaged) Jump.Invoke(context.started);
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed && !IsUIEngaged) Previous.Invoke();
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (context.performed && !IsUIEngaged) Next.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            if (!IsUIEngaged)
            {
                if (context.performed)
                    Sprint.Invoke(true);
                else if (context.canceled)
                    Sprint.Invoke(false);
            }
        }

        public void OnLean(InputAction.CallbackContext context)
        {
            if (context.performed || context.canceled)
                if (!IsUIEngaged)
                    Lean.Invoke(context.ReadValue<float>());
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            if (!IsUIEngaged)
            {
                if (context.performed)
                    Aim.Invoke(true);
                else if (context.canceled)
                    Aim.Invoke(false);
            }
        }

        public void OnSwitchItem(InputAction.CallbackContext context)
        {
            if (context.performed && !IsUIEngaged) SwitchItem.Invoke(context.ReadValue<float>());
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            if (context.performed && !IsUIEngaged) Reload.Invoke();
        }

        public void OnSwitchFireMode(InputAction.CallbackContext context)
        {
            if (context.performed && !IsUIEngaged) SwitchFireMode.Invoke();
        }

        public void OnRestart(InputAction.CallbackContext context)
        {
            if (context.performed) Restart.Invoke();
        }

        public void OnUIEngage(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (_uiState == UIStateType.InGame)
            {
                SetPlayerActionsEnabled(false);
                InputActions.UI.Enable();
                IsUIEngaged = true;
                UIEngaged.Invoke();
            }
            else
            {
                InputActions.UI.Disable();
                SetPlayerActionsEnabled(true);
                IsUIEngaged = false;
                UIDisengaged.Invoke();
            }
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
            if (context.performed) Cancel.Invoke();
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

        private void OnEnable()
        {
            EnablePlayerActions();
        }

        private void OnDestroy()
        {
            EventBus<UIStateSwitchedEvent>.Deregister(_uiStateChangedEventBinding);
            InputActions?.Disable();
        }

        private void OnUIStateChanged(UIStateSwitchedEvent evt)
        {
            _uiState = evt.NewState;

            if (evt.NewState == UIStateType.InGame)
            {
                InputActions.UI.Disable();
                SetPlayerActionsEnabled(true);
                IsUIEngaged = false;
            }
            else
            {
                SetPlayerActionsEnabled(false);
                InputActions.UI.Enable();
                IsUIEngaged = true;
            }
        }

        private void SetPlayerActionsEnabled(bool value)
        {
            var map = InputActions.asset.FindActionMap("Player");

            foreach (var action in map.actions)
            {
                if (action == InputActions.Player.UIEngage) continue;

                if (value)
                    action.Enable();
                else
                    action.Disable();
            }
        }

        private static bool IsMouse(InputAction.CallbackContext context)
        {
            return context.control.device is Mouse;
        }
    }

    public interface IInputReader
    {
        Vector2 Direction { get; }
        event UnityAction Attack;
        void EnablePlayerActions();
    }
}