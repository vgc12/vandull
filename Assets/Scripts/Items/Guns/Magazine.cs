using System;
using General;
using UnityEngine;

namespace Items.Guns
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Magazine : MonoBehaviour, IEquippable
    {
       
        public int CurrentAmmo { get; set; }
        public int Capacity { get; set; }
        
        public bool IsEmpty => CurrentAmmo <= 0;
        public bool IsFull => CurrentAmmo >= Capacity;
        
        public bool IsDropped { get; private set; }
        
        public Transform ParentTransform { get; set; }
        
        private Rigidbody _rigidbody;
        
        private Collider _collider;
        
        public MeshRenderer MeshRenderer { get; private set; }

        private AmmoSettings _ammoSettings;
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

        public void SubtractAmmo(int amount)
        {
            CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        }

        public void SubtractOne()
        {
            SubtractAmmo(1);
        }

        private void Update()
        {
            if (transform.parent != null)
            {
                transform.localPosition = _ammoSettings.magazinePosition;
            }
        }

        public void Equip()
        {
            if (CheckErrors()) return;
            MeshRenderer.enabled = true;
            transform.SetParent(ParentTransform);            
            transform.localPosition = _ammoSettings.magazinePosition;
            transform.localRotation = Quaternion.Euler(_ammoSettings.magazineRotation);
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
        
        public void Drop()
        {
            transform.SetParent(null);
            _rigidbody.isKinematic = false;
            _collider.enabled = true;
            IsDropped = true;
        }


        private bool CheckErrors()
        {
            bool hasError = false;
            if(AmmoSettings == null)
            {
                VandullLogger.LogError("AmmoSettings is not set on Magazine");
                hasError = true;
            }
            if(ParentTransform == null)
            {
                VandullLogger.LogError("ParentTransform is not set on Magazine");
                hasError = true;
            }
            return hasError;
            
        }
    }
    
    
}