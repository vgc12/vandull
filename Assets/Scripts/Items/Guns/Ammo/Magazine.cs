using Reflex.Attributes;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace Items.Guns.Ammo
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Magazine : MonoBehaviour, IEquippable
    {
        [Inject] private readonly ILogger _logger;

        private Collider _collider;
        private Transform _equipPosition;
        private MeshRenderer _meshRenderer;
        private Rigidbody _rigidbody;

        public int CurrentAmmo { get; set; }
        public int Capacity { get; set; }
        public bool IsEmpty => CurrentAmmo <= 0;
        public bool IsFull => CurrentAmmo >= Capacity;
        public bool IsDropped { get; private set; }

        private void Awake()
        {
            CacheComponents();
            SetPhysicsState(true, false);
        }

        public void Equip()
        {
            if (!IsInitialized()) return;

            transform.SetParent(_equipPosition);
            transform.SetPositionAndRotation(_equipPosition.position, _equipPosition.rotation);

            SetVisibility(true);
            SetPhysicsState(true, false);
            IsDropped = false;
        }

        public void UnEquip()
        {
            if (!IsInitialized()) return;

            transform.SetParent(null);
            SetVisibility(false);
            SetPhysicsState(true, false);
            IsDropped = false;
        }

        public void Initialize(AmmoSettings settings, Transform equipPosition)
        {
            if (!ValidateInitialization(settings, equipPosition)) return;

            _equipPosition = equipPosition;
            Capacity = settings.magazineSize;
            CurrentAmmo = Capacity;
        }

        public void Drop()
        {
            transform.SetParent(null);
            SetPhysicsState(false, true);
            IsDropped = true;
        }

        public void ConsumeAmmo(int amount = 1)
        {
            CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        }

        private void CacheComponents()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        private void SetPhysicsState(bool isKinematic, bool colliderEnabled)
        {
            if (_rigidbody != null)
                _rigidbody.isKinematic = isKinematic;

            if (_collider != null)
                _collider.enabled = colliderEnabled;
        }

        private void SetVisibility(bool visible)
        {
            if (_meshRenderer != null)
                _meshRenderer.enabled = visible;
        }

        private bool IsInitialized()
        {
            if (_equipPosition == null)
            {
                _logger?.LogError("Magazine not initialized - EquipPosition is null");
                return false;
            }

            return true;
        }

        private bool ValidateInitialization(AmmoSettings settings, Transform equipPosition)
        {
            var isValid = true;

            if (settings == null)
            {
                _logger?.LogError("AmmoSettings is null during Magazine initialization");
                isValid = false;
            }

            if (equipPosition == null)
            {
                _logger?.LogError("EquipPosition is null during Magazine initialization");
                isValid = false;
            }

            return isValid;
        }
    }
}