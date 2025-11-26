using Attributes;
using EventBus;
using Player.Input;
using Player.States;
using Reflex.Attributes;
using UnityEngine;

namespace Player.Looking
{
    /// <summary>
    /// Manages idle sway animation for objects (typically weapons or held items) to add subtle movement.
    /// Swaying is disabled when the player is aiming or not in an idle state.
    /// </summary>
    /// <remarks>
    /// This component uses sinusoidal functions to create smooth, natural-looking oscillation.
    /// It integrates with the EventBus system to respond to player state changes and uses
    /// dependency injection for input handling.
    /// </remarks>
    public sealed class ObjectSwayer : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Transform References")]
        [SerializeField]
        [Tooltip("The transform to apply sway motion to. If not set, uses this GameObject's transform.")]
        private Transform swayedObjectTransform;

        [SerializeField]
        [Tooltip("Speed at which the object interpolates to the target sway position. Higher values mean faster movement.")]
        private float swayLerpSpeed = 5f;

        [SerializeField]
        [Required]
        [Tooltip("Configuration asset containing all sway parameters (amplitude, frequency, multipliers).")]
        private SwayConfig sway;

        #endregion

        #region Injected Dependencies

        /// <summary>
        /// Player input interface injected via Reflex dependency injection.
        /// Used to subscribe to aim input events.
        /// </summary>
        [Inject] 
        private readonly IPlayerInput _input;

        #endregion

        #region Private Fields

        /// <summary>
        /// Flag indicating whether swaying is currently allowed based on player state.
        /// Set to true only when player is in IdleState.
        /// </summary>
        private bool _canSway = true;

        /// <summary>
        /// Event binding for player movement state changes.
        /// Automatically unregistered on destruction to prevent memory leaks.
        /// </summary>
        private EventBinding<PlayerMovementEnteredEvent> _event;

        /// <summary>
        /// Cached initial local position of the swayed object.
        /// Used as the center point for sway calculations and reset position.
        /// </summary>
        private Vector3 _initialLocalPosition;

        /// <summary>
        /// Flag indicating whether the player is currently aiming.
        /// When true, sway is disabled to provide stable aim.
        /// </summary>
        private bool _isAiming;

        /// <summary>
        /// Accumulating timer used for sinusoidal sway calculations.
        /// Continuously increments to drive the oscillation functions.
        /// </summary>
        private float _swayTimer;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Initializes the swayer by registering event listeners, caching references,
        /// and storing the initial position.
        /// </summary>
        private void Awake()
        {
            // Register for player movement state changes
            _event = new EventBinding<PlayerMovementEnteredEvent>(OnStateEntered);
            EventBus<PlayerMovementEnteredEvent>.Register(_event);
            
            // Subscribe to aim input events
            _input.Aim += OnAim;
            
            // Use this transform if no specific transform was assigned
            if (!swayedObjectTransform)
                swayedObjectTransform = transform;
                
            // Cache the starting position to serve as the sway origin
            _initialLocalPosition = swayedObjectTransform.localPosition;
        }

        /// <summary>
        /// Updates the sway animation each frame by either applying sway motion
        /// or smoothly returning to the initial position.
        /// </summary>
        private void Update()
        {
            _swayTimer += Time.deltaTime;
            
            if (_canSway && !_isAiming)
                Sway();
            else
                ResetSway();
        }

        /// <summary>
        /// Cleans up event subscriptions to prevent memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            EventBus<PlayerMovementEnteredEvent>.Deregister(_event);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles aim input state changes.
        /// </summary>
        /// <param name="arg0">True if the player is aiming, false otherwise.</param>
        private void OnAim(bool arg0)
        {
            _isAiming = arg0;
        }

        /// <summary>
        /// Handles player movement state changes to enable/disable swaying.
        /// Swaying is only enabled when the player enters the IdleState.
        /// </summary>
        /// <param name="e">Event containing information about the new movement state.</param>
        private void OnStateEntered(PlayerMovementEnteredEvent e)
        {
            _canSway = e.StateType == typeof(IdleState);
        }

        #endregion

        #region Sway Logic

        /// <summary>
        /// Smoothly interpolates the swayed object back to its initial local position.
        /// Called when swaying is disabled (player is moving or aiming).
        /// </summary>
        private void ResetSway()
        {
            swayedObjectTransform.localPosition = Vector3.Lerp(
                swayedObjectTransform.localPosition,
                _initialLocalPosition, 
                Time.deltaTime * swayLerpSpeed
            );
        }

        /// <summary>
        /// Applies sway motion to the object by calculating the target sway position
        /// and smoothly interpolating toward it.
        /// </summary>
        public void Sway()
        {
            var swayPosition = CalculateSwayPosition();
            swayedObjectTransform.localPosition = Vector3.Lerp(
                swayedObjectTransform.localPosition,
                swayPosition,
                Time.deltaTime * swayLerpSpeed
            );
        }

        /// <summary>
        /// Calculates the target sway position using sinusoidal functions for natural oscillation.
        /// </summary>
        /// <returns>
        /// A Vector3 representing the target local position with horizontal and vertical sway applied.
        /// The Z component is preserved from the current position.
        /// </returns>
        /// <remarks>
        /// Uses Cos for horizontal sway and Sin for vertical sway to create a figure-eight or elliptical motion.
        /// The sway amplitude is reduced by the aim multiplier when aiming (if applicable).
        /// </remarks>
        private Vector3 CalculateSwayPosition()
        {
            var multiplier = _isAiming ? sway.aimMultiplier : 1f;
            
            var horizontalSway = Mathf.Cos(_swayTimer * sway.horizontalSwaySpeed) *
                                 sway.horizontalSwayAmount * 
                                 sway.swayMultiplier * 
                                 multiplier;

            var verticalSway = Mathf.Sin(_swayTimer * sway.verticalSwaySpeed) *
                               sway.verticalSwayAmount * 
                               sway.swayMultiplier * 
                               multiplier;

            return new Vector3(
                horizontalSway,
                verticalSway,
                swayedObjectTransform.localPosition.z
            );
        }

        #endregion
    }
}