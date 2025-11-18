using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DependencyInjection;
using EventBus;
using Items;
using Items.Guns;
using JetBrains.Annotations;
using Levels;
using Levels.Strategies;
using Npcs.Sensors;
using Player;
using Shared;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using ILogger = General.Logging.ILogger;
using Image = UnityEngine.UI.Image;

namespace UI.States
{
    public class InGameUIState : UIBaseState
    {
        private const float DamageFadeDuration = 2f;
        private static readonly Color HighDetectionColor = new(1f, 0, 0, 1f);
        private static readonly Color LowDetectionColor = new(1f, 1f, 0f, 0.3f);
        private readonly Dictionary<RaycastObjectSensor, DirectionalIndicator> _activeDetectionIndicators = new();

        private readonly GameObject _crosshair;
        private readonly Color _damageColor = new(1f, 0.2f, 0.2f, 1f);


        private readonly ObjectPool<DirectionalIndicator> _damageIndicatorPool;
        private readonly GameObject _damageIndicatorPrefab;

        private readonly ObjectPool<DirectionalIndicator> _detectionIndicatorPool;
        private readonly GameObject _detectionIndicatorPrefab;

        private readonly EventBinding<DetectionMeterUpdatedEvent> _detectionMeterUpdatedEventBinding;
        private readonly Image _healthBar;

        private readonly GameObject _inGameUIRoot;
        private readonly EventBinding<ItemSwitchedEvent> _itemSwitchedEventBinding;
        private readonly ILogger _logger;
        private readonly EventBinding<PlayerHitEvent> _playerHitEventBinding;

        private Camera _cam;
        private CancellationTokenSource _cancellationTokenSource;
        private Gun _gun;
        private bool _isAiming;
        private IKillable _playerDamageable;
        private readonly EventBinding<EnemyKilledEvent> _enemyKilledEventBinding;

        public InGameUIState(VisualElement rootElement, GameObject inGameUI, UIStateMachine stateMachine) : base(
            rootElement, stateMachine, UIStateType.InGame)
        {
            _inGameUIRoot = inGameUI;

            _cancellationTokenSource = new CancellationTokenSource();

            foreach (Transform child in _inGameUIRoot.transform)
                switch (child.name)
                {
                    case "Crosshair":
                        _crosshair = child.gameObject;
                        break;
                    case "HealthBar":
                        _healthBar = child.GetComponentInChildren<Image>();
                        _healthBar.fillAmount = 1f;
                        break;
                    case "DamageIndicator":
                        _damageIndicatorPrefab = child.gameObject;
                        break;
                    case "DetectionIndicator":
                        _detectionIndicatorPrefab = child.gameObject;
                        break;
                }
            // Get a MonoBehaviour for running coroutines


            _detectionIndicatorPool = new ObjectPool<DirectionalIndicator>(
                () => CreateDirectionalIndicator(_detectionIndicatorPrefab),
                ind => ind.Container?.SetActive(true),
                ind => ind.Container?.SetActive(false),
                ind => Object.Destroy(ind.Container),
                false,
                5,
                5);

            _damageIndicatorPool = new ObjectPool<DirectionalIndicator>(
                () => CreateDamageIndicatorWrapper(_damageIndicatorPrefab),
                ind => ind.Container?.SetActive(true),
                ind => ind.Container?.SetActive(false),
                ind => Object.Destroy(ind.Container),
                false,
                10,
                20);

            _cam = Object.FindFirstObjectByType<Camera>();
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
            _logger.Log("Indicator system initialized");
        }

        private void OnEnemyKilled(EnemyKilledEvent obj)
        {
            if (_activeDetectionIndicators.Remove(obj.Enemy.PlayerSensor, out var indicator))
            {
                indicator.Container.SetActive(false);
                _detectionIndicatorPool.Release(indicator);
            }
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

        // Update OnSceneLoaded to clear the dictionary
        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _activeDetectionIndicators.Clear(); // Add this line
            _damageIndicatorPool.Clear();
            _detectionIndicatorPool.Clear();
            _cam = Object.FindFirstObjectByType<Camera>();
            _playerDamageable = null;
            _healthBar.fillAmount = PlayerDamageable?.Health / PlayerDamageable?.MaxHealth ?? 1;
        }

// Update OnSceneUnloaded to clear the dictionary
        private void OnSceneUnloaded(Scene arg0)
        {
            _cancellationTokenSource.Cancel();
            _activeDetectionIndicators.Clear(); // Add this line
            _damageIndicatorPool.Clear();
            _detectionIndicatorPool.Clear();
        }

        public override void Enter()
        {
            IsActive = true;
            _inGameUIRoot.SetActive(true);
            ChangeMouseState();
            EventBus<UIStateSwitchedEvent>.Raise(new UIStateSwitchedEvent(StateType));
        }

        private DamageIndicator CreateDamageIndicatorWrapper(GameObject prefab)
        {
            var container = Object.Instantiate(prefab, _inGameUIRoot.transform);
            return new DamageIndicator
            {
                Container = container,
                IndicatorImage = container.GetComponentInChildren<Image>()
            };
        }

        private DetectionIndicator CreateDirectionalIndicator(GameObject prefab)
        {
            var container = Object.Instantiate(prefab, _inGameUIRoot.transform);

            return new DetectionIndicator
            {
                Container = container,
                IndicatorImage = container.transform.Find("Image").GetComponent<Image>()
            };
        }

        // Replace your OnDetectionMeterUpdated method with this:
        private void OnDetectionMeterUpdated(DetectionMeterUpdatedEvent evt)
        {
            if (!IsActive)
            {
                return;
            }

            var threshold = evt.DetectionMeterMaximum * 0.01f;

            if (evt.Sensor.enabled && evt.DetectionMeter > threshold)
            {
                var meterProgress = evt.DetectionMeter / evt.DetectionMeterMaximum;

             
                // Get or create indicator for this sensor
                if (!_activeDetectionIndicators.TryGetValue(evt.Sensor, out var indicator))
                {
                    indicator = _detectionIndicatorPool.Get();
                    indicator.SourcePosition = evt.SensorTransform;
                    indicator.Container.SetActive(true);
                    _activeDetectionIndicators[evt.Sensor] = indicator;

                    // Start the continuous update coroutine
                    AnimateDetectionIndicator(indicator, evt.Sensor).Forget();
                }

                // Update the fill amount based on the meters normalized progress
                var detectionColor = Color.Lerp(LowDetectionColor, HighDetectionColor, meterProgress);
                detectionColor.a = meterProgress;
                indicator.IndicatorImage.color = detectionColor;
                indicator.IndicatorImage.fillAmount = meterProgress;
            }
            else
            {
                // Remove indicator when detection drops below threshold
                if (_activeDetectionIndicators.Remove(evt.Sensor, out var indicator))
                    _detectionIndicatorPool.Release(indicator);
            }
        }

// New method to continuously update indicator position/rotation
        private async UniTask AnimateDetectionIndicator(DirectionalIndicator indicator, RaycastObjectSensor sensor)
        {
            while (_activeDetectionIndicators.ContainsKey(sensor) &&
                   !_cancellationTokenSource.IsCancellationRequested)
            {
                // Check if source was destroyed
                if (indicator.SourcePosition == null || _cam == null)
                {
                    _activeDetectionIndicators.Remove(sensor);
                    _detectionIndicatorPool.Release(indicator);
                    return;
                }

                var newAngle = GetIndicatorAngle(indicator);

                indicator.Container.transform.localEulerAngles = new Vector3(0, 0, -newAngle);

                await UniTask.Yield(PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            }
        }

        private float GetIndicatorAngle(DirectionalIndicator indicator)
        {
            var playerPos = _cam.transform.position;
            var toSource = (indicator.SourcePosition.position - playerPos).normalized;

            // Project the direction onto the camera's view plane
            var projected = toSource - Vector3.Dot(toSource, _cam.transform.forward) * _cam.transform.forward;
            projected.Normalize();

            // Calculate angle using camera's right and up vectors
            var screenRight = Vector3.Dot(projected, _cam.transform.right);
            var screenUp = Vector3.Dot(projected, _cam.transform.up);
            var angle = Mathf.Atan2(screenRight, screenUp) * Mathf.Rad2Deg;

            // Smooth rotation
            var currentAngle = indicator.Container.transform.localEulerAngles.z;
            var angleDiff = Mathf.DeltaAngle(currentAngle, angle);
            var targetAngle = currentAngle + angleDiff;
            var newAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 300f);
            newAngle = Mathf.Repeat(newAngle + 180f, 360f) - 180f;
            return newAngle;
        }

        private void OnItemSwitched(ItemSwitchedEvent obj)
        {
            if (obj.NewItem is Gun gun)
                _gun = gun;
            else
                _gun = null;
        }

        public override void Update()
        {
            base.Update();
            _healthBar.fillAmount = PlayerDamageable?.Health / PlayerDamageable?.MaxHealth ?? 1;
            _isAiming = _gun != null && _gun.IsAiming;
            _crosshair.SetActive(!_isAiming);
        }

        private void OnPlayerHit(PlayerHitEvent obj)
        {
            _healthBar.fillAmount = obj.NewHealth / _playerDamageable.MaxHealth;
            _logger.Log($"Player hit! Health: {obj.NewHealth}, Hit source: {obj.DamageTransform}");

            if (obj.DamageTransform != null)
                ShowIndicator(_damageIndicatorPool, obj.DamageTransform, DamageFadeDuration, _damageColor).Forget();
            else
                _logger.LogWarning("PlayerHitEvent has no HitSource transform!");
        }

        private async UniTask ShowIndicator(
            ObjectPool<DirectionalIndicator> pool,
            Transform sourceTransform,
            float duration,
            Color color)
        {
            if (pool == null)
            {
                _logger.LogError("Cannot create indicator: pool is null");
                return;
            }

            var indicator = pool.Get();

            indicator.SourcePosition = sourceTransform;
            indicator.IndicatorImage.color = color;

            // Start the coroutine to update position/rotation and auto-return
            await AnimateDamageIndicator(indicator, pool, duration, color);
        }

        private async UniTask AnimateDamageIndicator(DirectionalIndicator indicator,
            ObjectPool<DirectionalIndicator> pool,
            float duration, Color baseColor)
        {
            var elapsed = 0f;

            while (elapsed < duration && !_cancellationTokenSource.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;

                var newAngle = GetIndicatorAngle(indicator);
                indicator.Container.transform.localEulerAngles = new Vector3(0, 0, -newAngle);


                var alpha = Mathf.Clamp01((duration - elapsed) / duration);
                indicator.IndicatorImage.color = new Color(
                    baseColor.r,
                    baseColor.g,
                    baseColor.b,
                    alpha * baseColor.a
                );


                await UniTask.Yield(PlayerLoopTiming.Update, _cancellationTokenSource.Token);
            }

            // Return to pool after duration
            pool.Release(indicator);
        }

        protected override void ChangeMouseState()
        {
            LockCursorAndHideMouse();
        }

        ~InGameUIState()
        {
            EventBus<PlayerHitEvent>.Deregister(_playerHitEventBinding);
            EventBus<ItemSwitchedEvent>.Deregister(_itemSwitchedEventBinding);
            EventBus<DetectionMeterUpdatedEvent>.Deregister(_detectionMeterUpdatedEventBinding);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public override void Exit()
        {
            IsActive = false;
            _inGameUIRoot.SetActive(false);
            UIStateMachine.ResetCommand();
        }

        private class DirectionalIndicator
        {
            public GameObject Container;
            public Image IndicatorImage;
            public Transform SourcePosition;
        }

        private class DamageIndicator : DirectionalIndicator
        {
        }

        private class DetectionIndicator : DirectionalIndicator
        {
        }
    }
}