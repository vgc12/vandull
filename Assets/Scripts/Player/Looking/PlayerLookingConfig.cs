using EventBus;
using UI.States;
using UnityEngine;

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

        private EventBinding<SettingsUIState.ControlSettingsChangedEvent> _controlSettingsChangedEventBinding;
        public float Sensitivity => sensitivity;
        public float LeanAngle => leanAngle;
        public float LeanSpeed => leanSpeed;

        private void OnEnable()
        {
            _controlSettingsChangedEventBinding =
                new EventBinding<SettingsUIState.ControlSettingsChangedEvent>(OnControlsChanged);
            EventBus<SettingsUIState.ControlSettingsChangedEvent>.Register(_controlSettingsChangedEventBinding);
        }

        private void OnDisable()
        {
            EventBus<SettingsUIState.ControlSettingsChangedEvent>.Deregister(_controlSettingsChangedEventBinding);
        }

        private void OnControlsChanged(SettingsUIState.ControlSettingsChangedEvent obj)
        {
            sensitivity = obj.MouseSensitivity * 10;
            xAimType = obj.InvertX ? AimType.Inverted : AimType.Normal;
            yAimType = obj.InvertY ? AimType.Inverted : AimType.Normal;
        }
    }
}