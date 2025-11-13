using System.Collections.Generic;
using EventBus;
using Items;
using Items.Guns;
using JetBrains.Annotations;
using Player;
using Shared;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI.States
{
    public class InGameUIState : UIBaseState
    {
        private readonly List<DamageIndicator> _activeIndicators = new();
        private readonly VisualElement _crosshair;
        private readonly Color _damageColor = new(1f, 0.2f, 0.2f, 0.8f);
        private readonly float _fadeDuration = 2f;
        private readonly ProgressBar _healthBar;
        private readonly VisualElement _indicatorContainer;

        // Damage indicator settings
        private readonly float _indicatorDistance = 150f;
        private readonly EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;
        private readonly EventBinding<PlayerHitEvent> _playerHitEventBinding;

        private Gun _gun;
        private bool _isAiming;
        private IKillable _playerDamageable;
        private Transform _playerTransform;

        public InGameUIState(VisualElement rootElement, UIStateMachine stateMachine) : base(rootElement, stateMachine,
            UIStateType.InGame)
        {
            _playerHitEventBinding = new EventBinding<PlayerHitEvent>(OnPlayerHit);
            _itemSwitchedEventBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);

            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedEventBinding);
            EventBus<PlayerHitEvent>.Register(_playerHitEventBinding);

            SceneManager.sceneLoaded += OnSceneLoaded;

            _healthBar = rootElement.Q<ProgressBar>("health-bar");
            _crosshair = rootElement.Q<VisualElement>("crosshair");

            // Setup damage indicator container
            _indicatorContainer = new VisualElement
            {
                name = "damage-indicator-container",
                pickingMode = PickingMode.Ignore
            };
            _indicatorContainer.style.position = Position.Absolute;
            _indicatorContainer.style.width = Length.Percent(100);
            _indicatorContainer.style.height = Length.Percent(100);
            _indicatorContainer.style.left = 0;
            _indicatorContainer.style.top = 0;

            rootElement.Add(_indicatorContainer);

            Debug.Log("Damage indicator system initialized");
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
                if (_playerDamageable != null)
                {
                    _playerTransform = ((MonoBehaviour)_playerDamageable).transform;
                    Debug.Log($"Found player at: {_playerTransform.position}");
                }

                return _playerDamageable;
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
            _playerDamageable = null;
            _playerTransform = null;
            _healthBar.value = PlayerDamageable?.Health ?? 100;

            ClearAllIndicators();
        }

        public override void Update()
        {
            base.Update();
            _healthBar.value = PlayerDamageable?.Health ?? 100;
            _crosshair.style.display = _isAiming ? DisplayStyle.None : DisplayStyle.Flex;
            _isAiming = _gun != null && _gun.IsAiming;

            UpdateDamageIndicators();
        }

        private void OnPlayerHit(PlayerHitEvent obj)
        {
            _healthBar.value = obj.NewHealth;

            Debug.Log($"Player hit! Health: {obj.NewHealth}, Hit source: {obj.DamageLocation}");

            // Show damage indicator if hit source position is available
            if (obj.DamageLocation != Vector3.zero)
            {
                ShowDamageIndicator(obj.DamageLocation);
            }
            else
            {
                Debug.LogWarning("PlayerHitEvent has no HitSource position!");
            }
        }

        private void ShowDamageIndicator(Vector3 damageSourcePosition)
        {
            Debug.Log($"Creating damage indicator for source at: {damageSourcePosition}");

            var indicator = CreateIndicatorElement();

            var ind = new DamageIndicator
            {
                Element = indicator,
                DamageSource = damageSourcePosition,
                Timer = _fadeDuration
            };

            _activeIndicators.Add(ind);
            _indicatorContainer.Add(indicator);

            Debug.Log($"Active indicators: {_activeIndicators.Count}");
        }

        private VisualElement CreateIndicatorElement()
        {
            var indicator = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };

            indicator.style.position = Position.Absolute;
            indicator.style.width = 64; // Adjust to your image size
            indicator.style.height = 64; // Adjust to your image size

            // Load your damage indicator image
            // Place your image in Resources folder: Assets/Resources/UI/DamageIndicator.png
            var indicatorTexture = Resources.Load<Texture2D>("UI/DamageIndicator");

            if (indicatorTexture != null)
            {
                indicator.style.backgroundImage = new StyleBackground(indicatorTexture);
                // Optional: tint the image with your damage color
                indicator.style.unityBackgroundImageTintColor = _damageColor;
            }
            else
            {
                Debug.LogError("Damage indicator texture not found! Check Resources/UI/Images/DamageIndicator.png");
                // Fallback to colored square if image not found
                indicator.style.backgroundColor = _damageColor;
            }

            return indicator;
        }

        private void UpdateDamageIndicators()
        {
            if (_activeIndicators.Count == 0)
            {
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("No main camera found!");
                return;
            }

            if (_playerTransform == null)
            {
                // Try to get player transform
                var _ = PlayerDamageable;
                if (_playerTransform == null)
                {
                    Debug.LogWarning("No player transform found!");
                    return;
                }
            }

            var playerPos = _playerTransform.position;
            var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            for (var i = _activeIndicators.Count - 1; i >= 0; i--)
            {
                var ind = _activeIndicators[i];
                ind.Timer -= Time.deltaTime;

                // Remove expired indicators
                if (ind.Timer <= 0)
                {
                    _indicatorContainer.Remove(ind.Element);
                    _activeIndicators.RemoveAt(i);
                    continue;
                }

                ind.DamageSource.y = playerPos.y; // Ignore vertical difference
                var toDamageSource = (ind.DamageSource - playerPos).normalized;

                var angle = Vector3.SignedAngle(toDamageSource, _playerTransform.forward, Vector3.up);
                var indicatorPos = screenCenter + new Vector2(
                    Mathf.Sin(angle * Mathf.Deg2Rad),
                    -Mathf.Cos(angle * Mathf.Deg2Rad)
                ) * _indicatorDistance;
                // Update position (center the 40x40 indicator)
                ind.Element.style.left = indicatorPos.x - 20;
                ind.Element.style.top = indicatorPos.y - 20;

                // Rotate to point toward damage source
                ind.Element.style.rotate = new Rotate(angle);

                // Fade out over time
                var alpha = Mathf.Clamp01(ind.Timer / _fadeDuration);
                var fadeColor = _damageColor;
                fadeColor.a = alpha * _damageColor.a;
                ind.Element.style.borderTopColor = fadeColor;
                ind.Element.style.opacity = alpha;
            }
        }

        private void ClearAllIndicators()
        {
            foreach (var indicator in _activeIndicators)
            {
                _indicatorContainer.Remove(indicator.Element);
            }

            _activeIndicators.Clear();
        }

        protected override void ChangeMouseState() { LockCursorAndHideMouse(); }

        ~InGameUIState()
        {
            EventBus<PlayerHitEvent>.Deregister(_playerHitEventBinding);
            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedEventBinding);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ClearAllIndicators();
        }

        private class DamageIndicator
        {
            public Vector3 DamageSource;
            public VisualElement Element;
            public float Timer;
        }
    }
}