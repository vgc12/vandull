using System.Collections.Generic;
using DependencyInjection;
using EventBus;
using Items;
using Items.Guns;
using JetBrains.Annotations;
using Npcs.Sensors;
using Player;
using Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ILogger = General.Logging.ILogger;

namespace UI.States
{
    public class InGameUIState : UIBaseState
    {
        private const int MaxIndicatorsPerType = 5;
        private const float DamageFadeDuration = 2f;
        private const float DetectionFadeDuration = 3f;

        private readonly VisualElement _crosshair;
        private readonly Color _damageColor = new(1f, 0.2f, 0.2f, 1f);

        private readonly Dictionary<Transform, DirectionalIndicator> _damageIndicators = new();
        private readonly Color _detectionColor = new(1f, 0.8f, 0f, 1f);
        private readonly Dictionary<Transform, DirectionalIndicator> _detectionIndicators = new();

        private readonly EventBinding<DetectionMeterUpdatedEvent> _detectionMeterUpdatedEventBinding;

        private readonly ProgressBar _healthBar;

        private readonly float _indicatorDistance = 150f;
        private readonly VisualElement _indicatorsRoot;
        private readonly VisualElement _indicatorTemplate;

        private readonly EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;
        private readonly ILogger _logger;
        private readonly EventBinding<PlayerHitEvent> _playerHitEventBinding;

        private Camera _cam;
        private Gun _gun;
        private bool _isAiming;
        private IKillable _playerDamageable;

        public InGameUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.InGame)
        {
            _cam = Object.FindFirstObjectByType<Camera>();
            _playerHitEventBinding = new EventBinding<PlayerHitEvent>(OnPlayerHit);
            _itemSwitchedEventBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);
            _detectionMeterUpdatedEventBinding = new EventBinding<DetectionMeterUpdatedEvent>(OnDetectionMeterUpdated);

            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedEventBinding);
            EventBus<PlayerHitEvent>.Register(_playerHitEventBinding);
            EventBus<DetectionMeterUpdatedEvent>.Register(_detectionMeterUpdatedEventBinding);

            SceneManager.sceneLoaded += OnSceneLoaded;

            _healthBar = rootElement.Q<VisualElement>("health-bar") as ProgressBar;
            _crosshair = rootElement.Q<VisualElement>("crosshair");

            // Get the template and root container
            _indicatorsRoot = rootElement;
            _indicatorTemplate = rootElement.Q<VisualElement>("IndicatorContainer");

            if (_indicatorTemplate == null)
            {
                Debug.LogError("IndicatorContainer not found in UI document!");
            }
            else
            {
                // Hide the template
                _indicatorTemplate.style.display = DisplayStyle.None;
            }

            RuntimeResolver.Instance.TryResolve(out _logger);

            _logger.Log("Indicator system initialized");
        }

        [CanBeNull]
        private IKillable PlayerDamageable
        {
            get
            {
                if (_playerDamageable != null)
                {
                    return _playerDamageable;
                }

                _playerDamageable = Object.FindFirstObjectByType<PlayerStateMachine>();
                return _playerDamageable;
            }
        }

        private void OnDetectionMeterUpdated(DetectionMeterUpdatedEvent evt)
        {
            // Only show indicator if detection is above threshold (e.g., 10% of max)
            var threshold = evt.DetectionMeterMaximum * 0.1f;

            if (evt.DetectionMeter > threshold)
            {
                var intensity = evt.DetectionMeter / evt.DetectionMeterMaximum;
                ShowOrUpdateDetectionIndicator(evt.SensorTransform, intensity);
            }
        }

        private void OnItemSwitched(ItemSwitchedEvent obj)
        {
            if (obj.NewItem is Gun gun)
            {
                _gun = gun;
            }
            else
            {
                _gun = null;
            }
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _cam = Object.FindFirstObjectByType<Camera>();
            _playerDamageable = null;
            _healthBar.value = PlayerDamageable?.Health ?? 100;

            ClearAllIndicators();
        }

        public override void Update()
        {
            base.Update();
            _healthBar.value = PlayerDamageable?.Health ?? 100;
            _crosshair.style.display = _isAiming ? DisplayStyle.None : DisplayStyle.Flex;
            _isAiming = _gun != null && _gun.IsAiming;

            UpdateIndicators(_damageIndicators);
            UpdateIndicators(_detectionIndicators);
        }

        private void OnPlayerHit(PlayerHitEvent obj)
        {
            _healthBar.value = obj.NewHealth;
            _logger.Log($"Player hit! Health: {obj.NewHealth}, Hit source: {obj.DamageTransform}");

            if (obj.DamageTransform != null)
            {
                ShowOrUpdateDamageIndicator(obj.DamageTransform);
            }
            else
            {
                _logger.LogWarning("PlayerHitEvent has no HitSource transform!");
            }
        }

        private void ShowOrUpdateDamageIndicator(Transform damageSource)
        {
            if (_damageIndicators.TryGetValue(damageSource, out var existingIndicator))
            {
                // Reset timer for existing indicator
                existingIndicator.Timer = DamageFadeDuration;
                _logger.Log($"Updated existing damage indicator for: {damageSource.name}");
            }
            else
            {
                // Create new indicator
                _logger.Log($"Creating new damage indicator for: {damageSource.name}");
                CreateIndicator(
                    _damageIndicators,
                    damageSource,
                    DamageFadeDuration,
                    _damageColor
                );
            }

            _logger.Log($"Active damage indicators: {_damageIndicators.Count}");
        }

        private void ShowOrUpdateDetectionIndicator(Transform detector, float intensity)
        {
            // Modulate color alpha based on detection intensity
            var detectionColor = _detectionColor;
            detectionColor.a = intensity;

            if (_detectionIndicators.TryGetValue(detector, out var existingIndicator))
            {
                // Update existing indicator color
                existingIndicator.Timer = DetectionFadeDuration;
                var indicatorImage = existingIndicator.Container.Q<VisualElement>("IndicatorImage");
                if (indicatorImage != null)
                {
                    indicatorImage.style.unityBackgroundImageTintColor = detectionColor;
                }

                _logger.Log($"Updated detection indicator for: {detector.name}, intensity: {intensity:F2}");
            }
            else
            {
                // Create new indicator
                _logger.Log($"Creating detection indicator for: {detector.name}, intensity: {intensity:F2}");
                CreateIndicator(
                    _detectionIndicators,
                    detector,
                    DetectionFadeDuration,
                    detectionColor
                );
            }

            _logger.Log($"Active detection indicators: {_detectionIndicators.Count}");
        }

        private void CreateIndicator(
            Dictionary<Transform, DirectionalIndicator> indicatorDict,
            Transform sourceTransform,
            float duration,
            Color color)
        {
            if (_indicatorTemplate == null)
            {
                _logger.LogError("Cannot create indicator: template is null");
                return;
            }

            // Remove oldest if at max capacity
            if (indicatorDict.Count >= MaxIndicatorsPerType)
            {
                Transform oldestKey = null;
                var oldestTimer = float.MaxValue;

                foreach (var kvp in indicatorDict)
                {
                    if (kvp.Value.Timer < oldestTimer)
                    {
                        oldestTimer = kvp.Value.Timer;
                        oldestKey = kvp.Key;
                    }
                }

                if (oldestKey != null)
                {
                    _indicatorsRoot.Remove(indicatorDict[oldestKey].Container);
                    indicatorDict.Remove(oldestKey);
                    _logger.Log("Removed oldest indicator to make room");
                }
            }

            // Clone the template container
            var container = new VisualElement
            {
                name = "IndicatorContainer-Clone",
                pickingMode = PickingMode.Ignore
            };

            // Add the USS class for styling
            container.AddToClassList("indicator-container");

            // Override position to absolute for dynamic positioning
            container.style.position = Position.Absolute;

            // Clone the indicator image child
            var indicatorImage = new VisualElement
            {
                name = "IndicatorImage",
                pickingMode = PickingMode.Ignore
            };

            // Add the USS class for styling
            indicatorImage.AddToClassList("indicator-image");

            // Apply the color tint
            indicatorImage.style.unityBackgroundImageTintColor = color;

            container.Add(indicatorImage);

            var indicator = new DirectionalIndicator
            {
                Container = container,
                SourcePosition = sourceTransform,
                Timer = duration,
                MaxDuration = duration
            };

            indicatorDict[sourceTransform] = indicator;
            _indicatorsRoot.Add(container);
        }

        private void UpdateIndicators(Dictionary<Transform, DirectionalIndicator> indicators)
        {
            if (indicators.Count == 0)
            {
                return;
            }

            if (_cam == null)
            {
                _logger.LogWarning("No main camera found!");
                return;
            }

            var playerPos = _cam.transform.position;
            var screenCenter = new Vector2(
                _indicatorsRoot.resolvedStyle.width / 2f,
                _indicatorsRoot.resolvedStyle.height / 2f
            );

            var keysToRemove = new List<Transform>();

            foreach (var kvp in indicators)
            {
                var transform = kvp.Key;
                var ind = kvp.Value;

                // Check if transform was destroyed
                if (transform == null)
                {
                    keysToRemove.Add(transform);
                    _indicatorsRoot.Remove(ind.Container);
                    continue;
                }

                ind.Timer -= Time.deltaTime;

                // Remove expired indicators
                if (ind.Timer <= 0)
                {
                    keysToRemove.Add(transform);
                    _indicatorsRoot.Remove(ind.Container);
                    continue;
                }


                var toSource = (transform.position - playerPos).normalized;

// Project the direction onto the camera's view plane (perpendicular to camera forward)
// This removes the depth component, leaving only the screen-space direction
                var projected = toSource - Vector3.Dot(toSource, _cam.transform.forward) * _cam.transform.forward;
                projected.Normalize();

// Calculate angle using camera's right and up vectors as reference frame
                var screenRight = Vector3.Dot(projected, _cam.transform.right);
                var screenUp = Vector3.Dot(projected, _cam.transform.up);
                var angle = Mathf.Atan2(screenRight, screenUp) * Mathf.Rad2Deg;

// Get current rotation angle
                var currentAngle = ind.Container.style.rotate.value.angle.value;

// Calculate the shortest angular distance
                var angleDiff = Mathf.DeltaAngle(currentAngle, angle);
                var targetAngle = currentAngle + angleDiff;

// Lerp using the shortest path
                var newAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 30f);

// Normalize to -180 to 180 range
                newAngle = Mathf.Repeat(newAngle + 180f, 360f) - 180f;

                ind.Container.style.rotate = new Rotate(newAngle);

// Fade out over time
                var alpha = Mathf.Clamp01(ind.Timer / ind.MaxDuration);
                ind.Container.style.opacity = alpha;
            }

            // Clean up removed indicators
            foreach (var key in keysToRemove)
            {
                indicators.Remove(key);
            }
        }

        private void ClearAllIndicators()
        {
            foreach (var kvp in _damageIndicators)
            {
                _indicatorsRoot.Remove(kvp.Value.Container);
            }

            _damageIndicators.Clear();

            foreach (var kvp in _detectionIndicators)
            {
                _indicatorsRoot.Remove(kvp.Value.Container);
            }

            _detectionIndicators.Clear();
        }

        protected override void ChangeMouseState() { LockCursorAndHideMouse(); }

        ~InGameUIState()
        {
            EventBus<PlayerHitEvent>.Deregister(_playerHitEventBinding);
            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedEventBinding);
            EventBus<DetectionMeterUpdatedEvent>.Deregister(_detectionMeterUpdatedEventBinding);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ClearAllIndicators();
        }

        private class DirectionalIndicator
        {
            public VisualElement Container;
            public float MaxDuration;
            public Transform SourcePosition;
            public float Timer;
        }
    }
}