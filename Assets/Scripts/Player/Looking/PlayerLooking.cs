using Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerLookingConfig", menuName = "Configs/Player/Movement/PlayerLookingConfig", order = 1)]
public class PlayerLookingConfig : ScriptableObject
{
    public float Sensitivity => sensitivity;
    public float LeanAngle => leanAngle;
    public float LeanSpeed => leanSpeed;

    [SerializeField] private float sensitivity = 50f;
    [SerializeField] private float leanSpeed = 10f;
    [SerializeField] private float leanAngle = 15f;
}


namespace Player.Looking
{
    [RequireComponent(typeof(GroundChecker), typeof(PlayerInput), typeof(Rigidbody))]
    [RequireComponent(typeof(CameraEffects))]
    public class PlayerLooking : MonoBehaviour
    {
        #region Variables

        private enum LeanDirection
        {
            Right = -1,
            Left = 1,
            None = 0
        }


        private PlayerInputActions _input;

        private Vector2 _mouseDelta;

        private LeanDirection _leanDirection = LeanDirection.None;

        private Vector2 _cameraRotation = Vector2.zero;


        private Rigidbody _rigidbody;

        public CameraEffects CameraEffects { get; private set; }

        [Header("Configuration")] [SerializeField, Required]
        private PlayerLookingConfig config;


        [Header("Transforms")] [SerializeField, Required]
        private Transform cameraHolder;

        [SerializeField, Required] private Transform orientation;

        [SerializeField, Required] private Transform leanPoint;

        [SerializeField, Required] private Transform cameraTransform;

        #endregion

        #region UnityFunctions

        private void Awake()
        {
            InitializeControls();
        }

        private void Start()
        {
            CameraEffects = GetComponent<CameraEffects>();
        }


        private void OnDestroy()
        {
            _input.Player.Look.performed -= OnMouseMove;
            _input.Player.Look.canceled -= OnMouseMove;
            _input.Player.Lean.started -= OnLeaning;
            _input.Player.Lean.canceled -= OnLeaning;
        }

        #endregion


        #region Controls

        private void InitializeControls()
        {
            _input = new PlayerInputActions();
            _input.Player.Enable();
            _input.Player.Look.performed += OnMouseMove;
            _input.Player.Look.canceled += OnMouseMove;


            _input.Player.Lean.started += OnLeaning;
            _input.Player.Lean.canceled += OnLeaning;
        }


        private void OnLeaning(InputAction.CallbackContext context)
        {
            _leanDirection = (LeanDirection)context.ReadValue<float>();
        }

        public void OnMouseMove(InputAction.CallbackContext context)
        {
            _mouseDelta = context.ReadValue<Vector2>();
        }

        #endregion


        #region StateMachineFunctions

        public void Lean()
        {
            leanPoint.rotation = Quaternion.Slerp(leanPoint.rotation,
                Quaternion.Euler(0, _cameraRotation.y, -(float)_leanDirection * config.LeanAngle),
                config.LeanSpeed * Time.deltaTime);
            cameraHolder.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
        }


        public void Look()
        {
            var mouseX = _mouseDelta.x * Time.deltaTime * config.Sensitivity;
            var mouseY = _mouseDelta.y * Time.deltaTime * config.Sensitivity;

            _cameraRotation.y += mouseX;
            _cameraRotation.x -= mouseY;

            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, -90f, 90f);

            cameraHolder.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
            orientation.rotation = Quaternion.Euler(0, _cameraRotation.y, 0);
        }

        #endregion
    }
}