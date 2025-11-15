using System.Collections.Generic;
using DependencyInjection;
using EventBus;
using Items;
using Items.Guns;
using JetBrains.Annotations;
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
        private const int MaxIndicators = 5;
        private const float FadeDuration = 2f;
        private readonly List<DamageIndicator> _activeIndicators = new(MaxIndicators);
        private readonly VisualElement _crosshair;
        private readonly Color _damageColor = new(1f, 0.2f, 0.2f, 0.8f);
        private readonly ProgressBar _healthBar;
        private readonly VisualElement _indicatorContainer;

        // Damage indicator settings
        private readonly float _indicatorDistance = 150f;
        private readonly Texture2D _indicatorTexture = Resources.Load<Texture2D>("UI/Images/DamageIndicator");
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

            RuntimeResolver.Instance.TryResolve(out _logger);

            rootElement.Add(_indicatorContainer);

            _logger.Log("Damage indicator system initialized");
        }

        [CanBeNull]
        private IKillable PlayerDamageable
        {
            get
            {
                if (_playerDamageable != null) return _playerDamageable;

                _playerDamageable = Object.FindFirstObjectByType<PlayerStateMachine>();


                return _playerDamageable;
            }
        }


        private void OnItemSwitched(ItemSwitchedEvent obj)
        {
            if (obj.NewItem is Gun gun)
                _gun = gun;
            else
                _gun = null;
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

            UpdateDamageIndicators();
        }

        private void OnPlayerHit(PlayerHitEvent obj)
        {
            _healthBar.value = obj.NewHealth;

            _logger.Log($"Player hit! Health: {obj.NewHealth}, Hit source: {obj.DamageLocation}");

            // Show damage indicator if hit source position is available
            if (obj.DamageLocation != Vector3.zero)
                ShowDamageIndicator(obj.DamageLocation);
            else
                _logger.LogWarning("PlayerHitEvent has no HitSource position!");
        }

        private void ShowDamageIndicator(Vector3 damageSourcePosition)
        {
            _logger.Log($"Creating damage indicator for source at: {damageSourcePosition}");

            var indicator = CreateIndicatorElement();

            var ind = new DamageIndicator
            {
                Element = indicator,
                DamageSource = damageSourcePosition,
                Timer = FadeDuration
            };

            _activeIndicators.Add(ind);
            _indicatorContainer.Add(indicator);

            _logger.Log($"Active indicators: {_activeIndicators.Count}");
        }

        private VisualElement CreateIndicatorElement()
        {
            if (_activeIndicators.Count >= MaxIndicators)
            {
                // Remove oldest indicator
                var oldest = _activeIndicators[0];
                _indicatorContainer.Remove(oldest.Element);
                _activeIndicators.RemoveAt(0);
            }

            var indicator = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };

            indicator.style.position = Position.Absolute;
            indicator.style.width = 128;
            indicator.style.height = 128;


            if (_indicatorTexture != null)
            {
                indicator.style.backgroundImage = new StyleBackground(_indicatorTexture);

                indicator.style.unityBackgroundImageTintColor = _damageColor;
            }
            else
            {
                _logger.LogError("Damage indicator texture not found! Check Resources/Images/UI/DamageIndicator.png");

                indicator.style.backgroundColor = _damageColor;
            }

            indicator.style.display = DisplayStyle.None;

            return indicator;
        }

        private void UpdateDamageIndicators()
        {
            if (_activeIndicators.Count == 0) return;

            if (_cam == null)
            {
                _logger.LogWarning("No main camera found!");
                return;
            }


            var playerPos = _cam.transform.position;
            // var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var screenCenter = new Vector2(
                _indicatorContainer.resolvedStyle.width / 2f,
                _indicatorContainer.resolvedStyle.height / 2f
            );

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

                // Flatten to horizontal plane for direction calculation
                var damageSourceFlat = ind.DamageSource;
                damageSourceFlat.y = playerPos.y;

                var toDamageSource = (damageSourceFlat - playerPos).normalized;

                // Calculate angle between player forward and damage source direction
                // Negative angle because UI coordinates have Y increasing downward
                var angle = Vector3.SignedAngle(_cam.transform.forward, toDamageSource, Vector3.up);

                // Calculate position on circle around screen center
                var indicatorPos = screenCenter + new Vector2(
                    Mathf.Sin(angle * Mathf.Deg2Rad),
                    -Mathf.Cos(angle * Mathf.Deg2Rad)
                ) * _indicatorDistance;

                // Center the indicator (256x256 image, so offset by half = 128)
                const float halfSize = 64f;
                ind.Element.style.left = indicatorPos.x - halfSize;
                ind.Element.style.top = indicatorPos.y - halfSize;

                // Rotate to point toward damage source
                // The indicator image should point upward by default, so angle directly maps
                ind.Element.style.rotate = new Rotate(angle);

                // Fade out over time
                var alpha = Mathf.Clamp01(ind.Timer / FadeDuration);
                ind.Element.style.display = DisplayStyle.Flex;
                ind.Element.style.opacity = alpha;
            }
        }

        private void ClearAllIndicators()
        {
            foreach (var indicator in _activeIndicators) _indicatorContainer.Remove(indicator.Element);

            _activeIndicators.Clear();
        }

        protected override void ChangeMouseState()
        {
            LockCursorAndHideMouse();
        }

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