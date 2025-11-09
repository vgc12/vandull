using EventBus;
using UI.States;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Looking
{
    [CreateAssetMenu(fileName = "PlayerLookingConfig", menuName = "Configs/Player/Movement/PlayerLookingConfig",
        order = 1)]
    public class PlayerLookingConfig : ScriptableObject
    {
        public enum AimType
        {
            Normal = 1,
            Inverted = -1
        }

        [SerializeField] private float sensitivity = 50f;
        [SerializeField] private float leanSpeed = 10f;
        [SerializeField] private float leanAngle = 15f;

        public AimType xAimType = AimType.Normal;
        public AimType yAimType = AimType.Normal;

        public float InputMultiplier;

        private EventBinding<SettingsUIState.ControlSettingsChangedEvent> _controlSettingsChangedEventBinding;

        private SettingsUIState.ControlScheme _currentControlScheme;

        public float Sensitivity => sensitivity;
        public float LeanAngle => leanAngle;
        public float LeanSpeed => leanSpeed;


        private void OnEnable()
        {
            InputSystem.onActionChange += OnActionChanged;


            _controlSettingsChangedEventBinding =
                new EventBinding<SettingsUIState.ControlSettingsChangedEvent>(OnControlsChanged);
            EventBus<SettingsUIState.ControlSettingsChangedEvent>.Register(_controlSettingsChangedEventBinding);
        }

        private void OnDisable()
        {
            InputSystem.onActionChange -= OnActionChanged;
            EventBus<SettingsUIState.ControlSettingsChangedEvent>.Deregister(_controlSettingsChangedEventBinding);
        }


        private void OnActionChanged(object obj, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed) return;
            if (obj is not InputAction action) return;
            var device = action.activeControl?.device;
            if (device != null) UpdateCurrentDeviceFromControl(device);
        }


        private void UpdateCurrentDeviceFromControl(InputDevice device)
        {
            InputMultiplier = 1.0f;

            if (device is not Keyboard && device is not Mouse)
                InputMultiplier = 10.0f;
        }


        private void OnControlsChanged(SettingsUIState.ControlSettingsChangedEvent obj)
        {
            sensitivity = obj.MouseSensitivity * 10;
            xAimType = obj.InvertX ? AimType.Inverted : AimType.Normal;
            yAimType = obj.InvertY ? AimType.Inverted : AimType.Normal;
        }
    }
}