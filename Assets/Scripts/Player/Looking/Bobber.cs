using System;
using System.Collections;
using System.Collections.Generic;
using Attributes;
using EventBus;
using Player.Input;
using Player.States;
using Reflex.Attributes;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace Player.Looking
{
    /// <summary>
    ///     Handles camera bobbing effects based on player movement state and aiming status.
    ///     Provides smooth transitions between different bob configurations.
    /// </summary>
    public class Bobber : MonoBehaviour
    {
        #region Dependencies

        [Inject] private readonly IPlayerInput _playerInput;
        [Inject] private ILogger _logger;

        #endregion

        #region Serialized Fields

        [Header("Transform References")]
        [SerializeField]
        [Tooltip("The transform that will be bobbed. If left empty, the GameObject's own transform will be used.")]
        private Transform bobTransform;

        [Header("Movement Settings")]
        [SerializeField]
        [Tooltip("Minimum velocity magnitude required to trigger bobbing")]
        private float movementThreshold = 0.1f;

        [Header("Lerp Settings")]
        [SerializeField]
        [Tooltip("Speed at which the bob position lerps to the target position during movement")]
        private float positionLerpSpeed = 1000f;

        [SerializeField] [Tooltip("Speed at which the bob position lerps back to rest position when stopping")]
        private float stopBobLerpSpeed = 10f;

        [SerializeField] [Tooltip("Distance threshold for snapping to target position")]
        private float positionSnapThreshold = 0.01f;

        [Header("Configuration")]
        [SerializeField]
        [Required]
        [Tooltip("ScriptableObject containing all bob configurations for different movement states")]
        private CameraBobConfig cameraBobConfig;

        #endregion

        #region Private Fields

        // Bob state
        private Dictionary<(Type stateType, bool isAiming), CameraBobSetting> _bobSettingsMap;
        private CameraBobSetting _currentSetting;
        private Type _currentStateType;
        private float _bobTimer;
        private Coroutine _bobCoroutine;

        // Transform state
        private Vector3 _initialPosition;

        // Player state
        private bool _isAiming;
        private Rigidbody _rigidbody;

        // Events
        private EventBinding<PlayerMovementEnteredEvent> _movementStateEvent;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeTransform();
            InitializeBobSettingsMap();
            RegisterEvents();
            SetDefaultBobSetting();
        }

        private void Update()
        {
            _logger.LogWarning(
                $"CurrentSetting: {_currentSetting != null} Rigidbody: {_rigidbody != null} CurrentStateType: {_currentStateType != null}");
            if (!ShouldBob())
            {
                StopBobbing();
                return;
            }

            Bob(_currentSetting, _rigidbody.linearVelocity);
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        #endregion

        #region Initialization

        /// <summary>
        ///     Initializes the bob transform and stores its initial local position.
        /// </summary>
        private void InitializeTransform()
        {
            if (!bobTransform) bobTransform = transform;

            _initialPosition = bobTransform.localPosition;
        }

        /// <summary>
        ///     Creates a mapping of movement states and aiming status to their corresponding bob settings.
        /// </summary>
        private void InitializeBobSettingsMap()
        {
            _bobSettingsMap = new Dictionary<(Type, bool), CameraBobSetting>
            {
                // Sprint states
                { (typeof(SprintState), false), cameraBobConfig.sprintConfig },
                { (typeof(SprintState), true), cameraBobConfig.sprintAimConfig },

                // Walk states
                { (typeof(WalkState), false), cameraBobConfig.walkConfig },
                { (typeof(WalkState), true), cameraBobConfig.aimWalkConfig },

                // Crouch walk states
                { (typeof(CrouchWalkState), false), cameraBobConfig.crouchWalkConfig },
                { (typeof(CrouchWalkState), true), cameraBobConfig.aimCrouchWalkConfig },

                // Idle states (no bobbing)
                { (typeof(IdleState), false), null },
                { (typeof(IdleState), true), null }
            };
        }

        /// <summary>
        ///     Registers all event listeners.
        /// </summary>
        private void RegisterEvents()
        {
            _movementStateEvent = new EventBinding<PlayerMovementEnteredEvent>(OnMovementStateChanged);
            EventBus<PlayerMovementEnteredEvent>.Register(_movementStateEvent);
            _playerInput.Aim += OnAim;
        }

        /// <summary>
        ///     Unregisters all event listeners.
        /// </summary>
        private void UnregisterEvents()
        {
            EventBus<PlayerMovementEnteredEvent>.Deregister(_movementStateEvent);
            _playerInput.Aim -= OnAim;
        }

        /// <summary>
        ///     Sets the initial bob setting to sprint configuration.
        /// </summary>
        private void SetDefaultBobSetting()
        {
            _currentSetting = cameraBobConfig.sprintConfig;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        ///     Handles changes to the player's aiming state.
        /// </summary>
        /// <param name="value">True if player is aiming, false otherwise</param>
        private void OnAim(bool value)
        {
            _isAiming = value;
            UpdateBobSetting(_currentStateType);
        }

        /// <summary>
        ///     Handles changes to the player's movement state.
        /// </summary>
        /// <param name="obj">Event containing the new movement state type</param>
        private void OnMovementStateChanged(PlayerMovementEnteredEvent obj)
        {
            _currentStateType = obj.StateType;
            _rigidbody = obj.Rigidbody;
            UpdateBobSetting(_currentStateType);
        }

        #endregion

        #region Bob Setting Management

        /// <summary>
        ///     Updates the current bob setting based on the movement state and aiming status.
        /// </summary>
        /// <param name="stateType">The current movement state type</param>
        private void UpdateBobSetting(Type stateType)
        {
            if (stateType == null) return;

            if (_bobSettingsMap.TryGetValue((stateType, _isAiming), out var setting)) _currentSetting = setting;
        }

        /// <summary>
        ///     Checks if bobbing should be active based on current state.
        /// </summary>
        /// <returns>True if bobbing should occur, false otherwise</returns>
        private bool ShouldBob()
        {
            return _currentSetting != null && _rigidbody != null && _currentStateType != null;
        }

        #endregion

        #region Bob Execution

        /// <summary>
        ///     Executes the bobbing motion based on the current bob setting and player velocity.
        /// </summary>
        /// <param name="cameraBobSetting">The bob configuration to use</param>
        /// <param name="velocity">The player's current velocity</param>
        public void Bob(CameraBobSetting cameraBobSetting, Vector3 velocity)
        {
            StopCurrentBobCoroutine();

            var horizontalSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
            var isMoving = horizontalSpeed > movementThreshold;

            if (!isMoving) return;

            UpdateBobTimer(cameraBobSetting, horizontalSpeed);
            ApplyBobMovement(cameraBobSetting, horizontalSpeed);
        }

        /// <summary>
        ///     Initiates a smooth transition back to the initial rest position.
        /// </summary>
        public void StopBobbing()
        {
            if (_bobCoroutine != null) return;

            _bobCoroutine = StartCoroutine(LerpToPosition(_initialPosition));
            _bobTimer = 0f;
        }

        /// <summary>
        ///     Stops any currently running bob coroutine.
        /// </summary>
        private void StopCurrentBobCoroutine()
        {
            if (_bobCoroutine == null) return;
            StopCoroutine(_bobCoroutine);
            _bobCoroutine = null;
        }

        /// <summary>
        ///     Updates the bob timer based on the bob frequency and player speed.
        /// </summary>
        /// <param name="cameraBobSetting">The bob configuration containing frequency settings</param>
        /// <param name="horizontalSpeed">The player's horizontal movement speed</param>
        private void UpdateBobTimer(CameraBobSetting cameraBobSetting, float horizontalSpeed)
        {
            _bobTimer += Time.deltaTime * cameraBobSetting.frequency *
                         Mathf.Min(horizontalSpeed, cameraBobSetting.maxSpeed);
        }

        /// <summary>
        ///     Calculates and applies the bob offset to the transform based on sine wave patterns.
        /// </summary>
        /// <param name="cameraBobSetting">The bob configuration containing amplitude and speed settings</param>
        /// <param name="horizontalSpeed">The player's horizontal movement speed</param>
        private void ApplyBobMovement(CameraBobSetting cameraBobSetting, float horizontalSpeed)
        {
            // Calculate horizontal bob using sine wave
            var horizontalBob = Mathf.Sin(_bobTimer) * cameraBobSetting.horizontalAmplitude;

            // Calculate vertical bob using faster sine wave (2x frequency for figure-8 pattern)
            var verticalBob = Mathf.Sin(_bobTimer * 2) * cameraBobSetting.verticalAmplitude;

            // Scale bob intensity based on speed
            var speedMultiplier = Mathf.Min(horizontalSpeed / cameraBobSetting.speedCurve, 1f);

            // Combine bob offsets
            var bobOffset = new Vector3(
                horizontalBob * speedMultiplier,
                verticalBob * speedMultiplier,
                0
            );

            // Apply smooth lerp to target position
            var targetPosition = _initialPosition + bobOffset;
            bobTransform.localPosition = Vector3.Lerp(
                bobTransform.localPosition,
                targetPosition,
                Time.deltaTime * positionLerpSpeed
            );
        }

        /// <summary>
        ///     Smoothly lerps the transform to a target position over time.
        /// </summary>
        /// <param name="targetPosition">The position to lerp towards</param>
        /// <returns>Coroutine enumerator</returns>
        private IEnumerator LerpToPosition(Vector3 targetPosition)
        {
            while (Vector3.Distance(bobTransform.localPosition, targetPosition) > positionSnapThreshold)
            {
                bobTransform.localPosition = Vector3.Lerp(
                    bobTransform.localPosition,
                    targetPosition,
                    Time.deltaTime * stopBobLerpSpeed
                );

                yield return null;
            }

            // Snap to final position
            bobTransform.localPosition = targetPosition;
            _bobCoroutine = null;
        }

        #endregion
    }
}