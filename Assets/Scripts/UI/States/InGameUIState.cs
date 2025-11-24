// ===================== MODELS =====================

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DependencyInjection;
using EventBus;
using Items;
using Items.Guns;
using JetBrains.Annotations;
using Levels.Strategies;
using Npcs.Sensors;
using Player;
using Shared;
using UI.InGame.Controllers;
using UI.InGame.Models;
using UI.InGame.Views;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;
using ILogger = General.Logging.ILogger;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

namespace UI.InGame.Models
{
    // Model: Holds data only, no logic
    public class HealthModel
    {
        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }
        public float HealthPercentage => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;

        public event Action<float, float> OnHealthChanged;

        public void UpdateHealth(float current, float max)
        {
            CurrentHealth = current;
            MaxHealth = max;
            OnHealthChanged?.Invoke(current, max);
        }
    }

    public class WeaponModel
    {
        public bool IsAiming { get; private set; }
        public bool HasWeapon { get; private set; }

        public event Action<bool> OnAimStateChanged;

        public void UpdateAimState(bool isAiming, bool hasWeapon)
        {
            IsAiming = isAiming;
            HasWeapon = hasWeapon;
            OnAimStateChanged?.Invoke(isAiming);
        }
    }

    public class IndicatorModel
    {
        public Transform SourcePosition { get; set; }
        public float Intensity { get; set; }
        public Color Color { get; set; }
        public float FillAmount { get; set; }
        public bool IsActive { get; set; }
    }

    public class DetectionIndicatorModel : IndicatorModel
    {
        public LineOfSightSensor SensorKey { get; set; } // RaycastObjectSensor
    }

    public class DamageIndicatorModel : IndicatorModel
    {
        public float RemainingDuration { get; set; }
        public float TotalDuration { get; set; }
    }
}

// ===================== VIEWS =====================


namespace UI.InGame.Views
{
    // View: Handles rendering and UI updates only
    public interface IInGameView
    {
        void UpdateHealthBar(float fillAmount);
        void SetCrosshairActive(bool active);
    }

    public class InGameView : IInGameView
    {
        private readonly GameObject _crosshair;
        private readonly Image _healthBar;

        public InGameView(Image healthBar, GameObject crosshair)
        {
            _healthBar = healthBar;
            _crosshair = crosshair;
        }

        public void UpdateHealthBar(float fillAmount)
        {
            if (_healthBar != null)
                _healthBar.fillAmount = fillAmount;
        }

        public void SetCrosshairActive(bool active)
        {
            if (_crosshair != null)
                _crosshair.SetActive(active);
        }
    }

    public interface IIndicatorView
    {
        GameObject Container { get; }
        void UpdateRotation(float angle);
        void UpdateColor(Color color);
        void UpdateFillAmount(float amount);
        void SetActive(bool active);
    }

    public class DirectionalIndicatorView : IIndicatorView
    {
        private readonly Image _indicatorImage;

        public DirectionalIndicatorView(GameObject container, Image indicatorImage)
        {
            Container = container;
            _indicatorImage = indicatorImage;
        }

        public GameObject Container { get; }

        public void UpdateRotation(float angle)
        {
            Container.transform.localEulerAngles = new Vector3(0, 0, -angle);
        }

        public void UpdateColor(Color color)
        {
            if (_indicatorImage != null)
                _indicatorImage.color = color;
        }

        public void UpdateFillAmount(float amount)
        {
            if (_indicatorImage != null)
                _indicatorImage.fillAmount = amount;
        }

        public void SetActive(bool active)
        {
            Container.SetActive(active);
        }
    }
}

// ===================== CONTROLLERS =====================

namespace UI.InGame.Controllers
{
    // Controller: Handles business logic and coordinates Model-View
    public class HealthController
    {
        private readonly HealthModel _model;
        private readonly IInGameView _view;

        public HealthController(HealthModel model, IInGameView view)
        {
            _model = model;
            _view = view;
            _model.OnHealthChanged += OnHealthChanged;
        }

        private void OnHealthChanged(float current, float max)
        {
            _view.UpdateHealthBar(_model.HealthPercentage);
        }

        public void UpdateHealth(float current, float max)
        {
            _model.UpdateHealth(current, max);
        }

        public void Dispose()
        {
            _model.OnHealthChanged -= OnHealthChanged;
        }
    }

    public class WeaponController
    {
        private readonly WeaponModel _model;
        private readonly IInGameView _view;

        public WeaponController(WeaponModel model, IInGameView view)
        {
            _model = model;
            _view = view;
            _model.OnAimStateChanged += OnAimStateChanged;
        }

        private void OnAimStateChanged(bool isAiming)
        {
            _view.SetCrosshairActive(!isAiming);
        }

        public void UpdateWeaponState(bool isAiming, bool hasWeapon)
        {
            _model.UpdateAimState(isAiming, hasWeapon);
        }

        public void Dispose()
        {
            _model.OnAimStateChanged -= OnAimStateChanged;
        }
    }

    public class IndicatorController
    {
        private readonly Camera _camera;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public IndicatorController(Camera camera, CancellationTokenSource cancellationTokenSource)
        {
            _camera = camera;
            _cancellationTokenSource = cancellationTokenSource;
        }

        public float CalculateIndicatorAngle(Transform sourcePosition, GameObject indicatorContainer)
        {
            if (_camera == null || sourcePosition == null) return 0f;

            var playerPos = _camera.transform.position;
            var toSource = (sourcePosition.position - playerPos).normalized;

            // Project onto camera view plane
            var projected = toSource - Vector3.Dot(toSource, _camera.transform.forward) * _camera.transform.forward;
            projected.Normalize();

            // Calculate screen-space angle
            var screenRight = Vector3.Dot(projected, _camera.transform.right);
            var screenUp = Vector3.Dot(projected, _camera.transform.up);
            var angle = Mathf.Atan2(screenRight, screenUp) * Mathf.Rad2Deg;

            // Smooth rotation
            var currentAngle = indicatorContainer.transform.localEulerAngles.z;
            var angleDiff = Mathf.DeltaAngle(currentAngle, angle);
            var newAngle = Mathf.Lerp(currentAngle, currentAngle + angleDiff, Time.deltaTime * 300f);
            return Mathf.Repeat(newAngle + 180f, 360f) - 180f;
        }

        public async UniTask AnimateDamageIndicator(
            DamageIndicatorModel model,
            IIndicatorView view,
            Color baseColor)
        {
            var elapsed = 0f;

            while (elapsed < model.TotalDuration && !_cancellationTokenSource.IsCancellationRequested)
            {
                if (model.SourcePosition == null || _camera == null)
                    return;

                elapsed += Time.deltaTime;
                model.RemainingDuration = model.TotalDuration - elapsed;

                // Update rotation
                var angle = CalculateIndicatorAngle(model.SourcePosition, view.Container);
                view.UpdateRotation(angle);

                // Update fade
                var alpha = Mathf.Clamp01(model.RemainingDuration / model.TotalDuration);
                var color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha * baseColor.a);
                view.UpdateColor(color);

                await UniTask.Yield(PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            }
        }

        public async UniTask AnimateDetectionIndicator(
            DetectionIndicatorModel model,
            IIndicatorView view,
            Func<bool> shouldContinue)
        {
            while (shouldContinue() && !_cancellationTokenSource.IsCancellationRequested)
            {
                if (model.SourcePosition == null || _camera == null)
                    return;

                var angle = CalculateIndicatorAngle(model.SourcePosition, view.Container);
                view.UpdateRotation(angle);

                await UniTask.Yield(PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            }
        }

        public void UpdateDetectionIndicatorAppearance(
            DetectionIndicatorModel model,
            IIndicatorView view,
            float intensity,
            Color lowColor,
            Color highColor)
        {
            model.Intensity = intensity;
            var color = Color.Lerp(lowColor, highColor, intensity);
            color.a = intensity;
            model.Color = color;
            model.FillAmount = intensity;

            view.UpdateColor(model.Color);
            view.UpdateFillAmount(model.FillAmount);
        }
    }
}

// ===================== MAIN STATE (MVC COORDINATOR) =====================


namespace UI.States
{
    // This is now just the coordinator that wires up MVC components
    public class InGameUIState : UIBaseState
    {
        private const float DamageFadeDuration = 2f;
        private static readonly Color HighDetectionColor = new(1f, 0, 0, 1f);
        private static readonly Color LowDetectionColor = new(1f, 1f, 0f, 0.3f);

        // Indicator management
        private readonly Dictionary<ISensor, (DetectionIndicatorModel model, IIndicatorView view)>
            _activeDetectionIndicators = new();

        private readonly Color _damageColor = new(1f, 0.2f, 0.2f, 1f);
        private readonly ObjectPool<IIndicatorView> _damageIndicatorPool;
        private readonly GameObject _damageIndicatorPrefab;
        private readonly ObjectPool<IIndicatorView> _detectionIndicatorPool;
        private readonly GameObject _detectionIndicatorPrefab;
        private readonly EventBinding<DetectionMeterUpdatedEvent> _detectionMeterUpdatedEventBinding;
        private readonly EventBinding<EnemyKilledEvent> _enemyKilledEventBinding;
        private readonly HealthController _healthController;

        // MVC Components
        private readonly HealthModel _healthModel;

        private readonly GameObject _inGameUIRoot;
        private readonly EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;
        private readonly ILogger _logger;
        private readonly InGameView _mainView;

        // Event bindings
        private readonly EventBinding<PlayerHitEvent> _playerHitEventBinding;
        private readonly WeaponController _weaponController;
        private readonly WeaponModel _weaponModel;

        private Camera _cam;
        private CancellationTokenSource _cancellationTokenSource;
        private Gun _gun;
        private IndicatorController _indicatorController;
        private IKillable _playerDamageable;

        public InGameUIState(VisualElement rootElement, GameObject inGameUI, UIStateMachine stateMachine)
            : base(rootElement, stateMachine, UIStateType.InGame)
        {
            _inGameUIRoot = inGameUI;
            _cancellationTokenSource = new CancellationTokenSource();

            // Initialize models
            _healthModel = new HealthModel();
            _weaponModel = new WeaponModel();

            // Find UI components and create view
            GameObject crosshair = null;
            Image healthBar = null;

            foreach (Transform child in _inGameUIRoot.transform)
                switch (child.name)
                {
                    case "Crosshair":
                        crosshair = child.gameObject;
                        break;
                    case "HealthBar":
                        healthBar = child.GetComponentInChildren<Image>();
                        healthBar.fillAmount = 1f;
                        break;
                    case "DamageIndicator":
                        _damageIndicatorPrefab = child.gameObject;
                        break;
                    case "DetectionIndicator":
                        _detectionIndicatorPrefab = child.gameObject;
                        break;
                }

            _mainView = new InGameView(healthBar, crosshair);
            _cam = Object.FindFirstObjectByType<Camera>();

            // Initialize controllers
            _healthController = new HealthController(_healthModel, _mainView);
            _weaponController = new WeaponController(_weaponModel, _mainView);
            _indicatorController = new IndicatorController(_cam, _cancellationTokenSource);

            // Initialize pools
            _detectionIndicatorPool = new ObjectPool<IIndicatorView>(
                () => CreateIndicatorView(_detectionIndicatorPrefab),
                view => view.SetActive(true),
                view => view.SetActive(false),
                view => Object.Destroy(view.Container),
                false, 5, 5);

            _damageIndicatorPool = new ObjectPool<IIndicatorView>(
                () => CreateIndicatorView(_damageIndicatorPrefab),
                view => view.SetActive(true),
                view => view.SetActive(false),
                view => Object.Destroy(view.Container),
                false, 10, 20);

            // Register event handlers
            _playerHitEventBinding = new EventBinding<PlayerHitEvent>(OnPlayerHit);
            _itemSwitchedEventBinding = new EventBinding<ItemSwitchedEvent>(OnItemSwitched);
            _detectionMeterUpdatedEventBinding = new EventBinding<DetectionMeterUpdatedEvent>(OnDetectionMeterUpdated);
            _enemyKilledEventBinding = new EventBinding<EnemyKilledEvent>(OnEnemyKilled);

            EventBus<ItemSwitchedEvent>.Register(_itemSwitchedEventBinding);
            EventBus<PlayerHitEvent>.Register(_playerHitEventBinding);
            EventBus<DetectionMeterUpdatedEvent>.Register(_detectionMeterUpdatedEventBinding);
            EventBus<EnemyKilledEvent>.Register(_enemyKilledEventBinding);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            RuntimeResolver.Instance.TryResolve(out _logger);
            _logger?.Log("Indicator system initialized");
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

        private IIndicatorView CreateIndicatorView(GameObject prefab)
        {
            var container = Object.Instantiate(prefab, _inGameUIRoot.transform);

            var image = prefab == _detectionIndicatorPrefab
                ? container.transform.Find("Image").GetComponent<Image>()
                : container.GetComponentInChildren<Image>();
            return new DirectionalIndicatorView(container, image);
        }


        public override void Enter()
        {
            IsActive = true;
            _inGameUIRoot.SetActive(true);
            ChangeMouseState();
            EventBus<UIStateSwitchedEvent>.Raise(new UIStateSwitchedEvent(StateType));
        }

        public override void Exit()
        {
            IsActive = false;
            _inGameUIRoot.SetActive(false);
            UIStateMachine.ResetCommand();
        }

        public override void Update()
        {
            base.Update();

            // Update models with current data
            var player = PlayerDamageable;
            if (player != null)
                _healthController.UpdateHealth(player.Health, player.MaxHealth);

            _weaponController.UpdateWeaponState(_gun != null && _gun.IsAiming, _gun != null);
        }

        private void OnPlayerHit(PlayerHitEvent evt)
        {
            _healthController.UpdateHealth(evt.NewHealth, _playerDamageable.MaxHealth);
            _logger?.Log($"Player hit! Health: {evt.NewHealth}, Hit source: {evt.DamageTransform}");

            if (evt.DamageTransform != null)
                ShowDamageIndicator(evt.DamageTransform).Forget();
            else
                _logger?.LogWarning("PlayerHitEvent has no HitSource transform!");
        }

        private async UniTask ShowDamageIndicator(Transform damageSource)
        {
            var view = _damageIndicatorPool.Get();
            var model = new DamageIndicatorModel
            {
                SourcePosition = damageSource,
                TotalDuration = DamageFadeDuration,
                RemainingDuration = DamageFadeDuration,
                Color = _damageColor,
                IsActive = true
            };

            view.UpdateColor(_damageColor);
            await _indicatorController.AnimateDamageIndicator(model, view, _damageColor);
            _damageIndicatorPool.Release(view);
        }

        private void OnDetectionMeterUpdated(DetectionMeterUpdatedEvent evt)
        {
            if (!IsActive) return;

            var threshold = evt.DetectionMeterMaximum * 0.01f;

            if (evt.Sensor.enabled && evt.DetectionMeter > threshold)
                UpdateDetectionIndicator(evt);
            else
                RemoveDetectionIndicator(evt.Sensor);
        }

        private void UpdateDetectionIndicator(DetectionMeterUpdatedEvent evt)
        {
            var intensity = evt.DetectionMeter / evt.DetectionMeterMaximum;

            if (!_activeDetectionIndicators.TryGetValue(evt.Sensor, out var indicator))
            {
                var view = _detectionIndicatorPool.Get();
                var model = new DetectionIndicatorModel
                {
                    SensorKey = evt.Sensor,
                    SourcePosition = evt.SensorTransform,
                    IsActive = true
                };

                indicator = (model, view);
                _activeDetectionIndicators[evt.Sensor] = indicator;
                view.SetActive(true);

                AnimateDetectionIndicator(evt.Sensor).Forget();
            }

            _indicatorController.UpdateDetectionIndicatorAppearance(
                indicator.model,
                indicator.view,
                intensity,
                LowDetectionColor,
                HighDetectionColor);
        }

        private async UniTask AnimateDetectionIndicator(LineOfSightSensor sensor)
        {
            if (!_activeDetectionIndicators.TryGetValue(sensor, out var indicator))
                return;

            await _indicatorController.AnimateDetectionIndicator(
                indicator.model,
                indicator.view,
                () => _activeDetectionIndicators.ContainsKey(sensor));

            RemoveDetectionIndicator(sensor);
        }

        private void RemoveDetectionIndicator(ISensor sensor)
        {
            if (_activeDetectionIndicators.Remove(sensor, out var indicator))
            {
                indicator.view.SetActive(false);
                _detectionIndicatorPool.Release(indicator.view);
            }
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            RemoveDetectionIndicator(evt.Enemy.PlayerSensor);
        }

        private void OnItemSwitched(ItemSwitchedEvent evt)
        {
            _gun = evt.NewItem as Gun;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _activeDetectionIndicators.Clear();
            _damageIndicatorPool.Clear();
            _detectionIndicatorPool.Clear();
            _cam = Object.FindFirstObjectByType<Camera>();
            _playerDamageable = null;

            // Reinitialize controller with new camera
            _indicatorController = new IndicatorController(_cam, _cancellationTokenSource);

            var player = PlayerDamageable;
            if (player != null)
                _healthController.UpdateHealth(player.Health, player.MaxHealth);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            _cancellationTokenSource.Cancel();
            _activeDetectionIndicators.Clear();
            _damageIndicatorPool.Clear();
            _detectionIndicatorPool.Clear();
        }

        protected override void ChangeMouseState()
        {
            LockCursorAndHideMouse();
        }

        ~InGameUIState()
        {
            _healthController?.Dispose();
            _weaponController?.Dispose();

            EventBus<PlayerHitEvent>.Deregister(_playerHitEventBinding);
            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedEventBinding);
            EventBus<DetectionMeterUpdatedEvent>.Deregister(_detectionMeterUpdatedEventBinding);
            EventBus<EnemyKilledEvent>.Deregister(_enemyKilledEventBinding);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }
}