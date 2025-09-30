using General;
using UnityEngine;

namespace Items.Guns.Ammo
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Magazine : MonoBehaviour, IEquippable
    {
        private AmmoSettings _ammoSettings;

        private Collider _collider;

        private Rigidbody _rigidbody;

        public int CurrentAmmo { get; set; }
        public int Capacity { get; set; }


        public bool IsEmpty => CurrentAmmo <= 0;
        public bool IsFull => CurrentAmmo >= Capacity;

        public bool IsDropped { get; private set; }

        public Transform MagazinePosition { get; set; }

        public MeshRenderer MeshRenderer { get; private set; }

        public AmmoSettings AmmoSettings
        {
            get => _ammoSettings;
            set
            {
                _ammoSettings = value;
                Capacity = _ammoSettings.magazineSize;
                CurrentAmmo = Capacity;
            }
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            MeshRenderer = GetComponentInChildren<MeshRenderer>();
            _collider.enabled = false;
            _rigidbody.isKinematic = true;
            IsDropped = false;
        }

        private void Update()
        {
            if (transform.parent != null) transform.position = MagazinePosition.position;
        }

        public void Equip()
        {
            if (CheckErrors()) return;
            MeshRenderer.enabled = true;
            transform.SetParent(MagazinePosition);
            transform.rotation = MagazinePosition.rotation;
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            IsDropped = false;
        }

        public void UnEquip()
        {
            if (CheckErrors()) return;
            transform.SetParent(null);
            MeshRenderer.enabled = false;
            _rigidbody.isKinematic = true;
            _collider.enabled = false;
            IsDropped = false;
        }

        public void SubtractAmmo(int amount)
        {
            CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        }

        public void SubtractOne()
        {
            SubtractAmmo(1);
        }

        public void Drop()
        {
            transform.SetParent(null);
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
            IsDropped = true;
        }


        private bool CheckErrors()
        {
            var hasError = false;
            if (AmmoSettings == null)
            {
                VandullLogger.LogError("AmmoSettings is not set on Magazine");
                hasError = true;
            }

            if (MagazinePosition == null)
            {
                VandullLogger.LogError("MagazineTransform is not set on Magazine");
                hasError = true;
            }

            return hasError;
        }
    }
}