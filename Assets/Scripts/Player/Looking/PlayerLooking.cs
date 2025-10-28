using Attributes;
using Player.Input;
using Reflex.Attributes;
using UnityEngine;

namespace Player.Looking
{
    [RequireComponent(typeof(GroundChecker), typeof(Rigidbody))]
    public class PlayerLooking : MonoBehaviour
    {
        #region Variables

        [Inject] private IPlayerInput _input;

        private enum LeanDirection
        {
            Right = -1,
            Left = 1,
            None = 0
        }

        [Required] public ObjectSwayer objectSwayer;

        private Vector2 _mouseDelta;

        private LeanDirection _leanDirection = LeanDirection.None;

        private Vector2 _cameraRotation = Vector2.zero;


        private Rigidbody _rigidbody;


        [Header("Configuration")] [Required] [SerializeField]
        private PlayerLookingConfig config;

        [Required] public SwayConfig swayConfig;


        [Header("Transforms")] [SerializeField] [Required]
        private Transform cameraHolder;

        [SerializeField] [Required] private Transform orientation;

        [SerializeField] [Required] private Transform leanPoint;

        [SerializeField] private Transform itemHolder;

        [Header("Configuration")] [Required] public CameraBobConfig weaponBobConfig;

        #endregion

        #region UnityFunctions

        private void Start()
        {
            InitializeControls();
        }


        private void OnDestroy()
        {
            _input.Look -= OnMouseMove;
            _input.Lean -= OnLeaning;
        }

        #endregion


        #region Controls

        private void InitializeControls()
        {
            _input.Look += OnMouseMove;

            _input.Lean += OnLeaning;
        }


        private void OnLeaning(float value)
        {
            _leanDirection = (LeanDirection)value;
        }

        public void OnMouseMove(Vector2 direction)
        {
            _mouseDelta = direction;
        }

        #endregion


        #region StateMachineFunctions

        public void Lean()
        {
            var rot = Quaternion.Slerp(leanPoint.rotation,
                Quaternion.Euler(0, _cameraRotation.y, -(float)_leanDirection * config.LeanAngle),
                config.LeanSpeed * Time.deltaTime);

            leanPoint.rotation = rot;
            cameraHolder.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
            if (!itemHolder) return;
            itemHolder.localRotation = Quaternion.Euler(itemHolder.localRotation.eulerAngles.x,
                itemHolder.localRotation.eulerAngles.x, leanPoint.rotation.eulerAngles.z);
        }


        public void Look()
        {
            var mouseX = _mouseDelta.x * Time.deltaTime * config.Sensitivity * (int)config.xAimType;
            var mouseY = _mouseDelta.y * Time.deltaTime * config.Sensitivity * (int)config.yAimType;

            _cameraRotation.y += mouseX;
            _cameraRotation.x -= mouseY;

            _cameraRotation.x = Mathf.Clamp(_cameraRotation.x, -90f, 90f);

            cameraHolder.rotation = Quaternion.Euler(_cameraRotation.x, _cameraRotation.y, 0);
            orientation.rotation = Quaternion.Euler(0, _cameraRotation.y, 0);
        }

        #endregion
    }
}