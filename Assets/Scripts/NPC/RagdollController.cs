using System.Collections;
using UnityEngine;

namespace NPC
{
    public class RagdollController : MonoBehaviour
    {
        [System.Serializable]
        public class RagdollBone
        {
            public Transform transform;
            public Rigidbody rigidbody;
            public Collider collider;
            public Vector3 storedPosition;
            public Quaternion storedRotation;
        }

        [SerializeField] private Animator animator;
        [SerializeField] private RagdollBone[] ragdollBones;
        [SerializeField] private float transitionDuration = 0.5f;

    
        private bool _isRagdollActive = false;
        private bool _isTransitioning = false;

        void Start()
        {

            if (ragdollBones.Length == 0)
                SetupRagdollBones();
        
            SetRagdollState(false);
        }

        private void SetupRagdollBones()
        {
            Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
            
            ragdollBones = new RagdollBone[rbs.Length];
        
            for (int i = 0; i < rbs.Length; i++)
            {
                ragdollBones[i] = new RagdollBone
                {
                    transform = rbs[i].transform,
                    rigidbody = rbs[i],
                    collider = rbs[i].GetComponent<Collider>()
                };
            }
        }

        public void ToggleRagdoll()
        {
            if (_isTransitioning) return;
        
            if (_isRagdollActive)
                StartCoroutine(TransitionFromRagdoll());
            else
                StartCoroutine(TransitionToRagdoll());
        }

        public void EnableRagdoll(bool immediate = false)
        {
            if (immediate)
                SetRagdollState(true);
            else
                StartCoroutine(TransitionToRagdoll());
        }

        public void DisableRagdoll(bool immediate = false)
        {
            if (immediate)
                SetRagdollState(false);
            else
                StartCoroutine(TransitionFromRagdoll());
        }

        private IEnumerator TransitionToRagdoll()
        {
            _isTransitioning = true;
        
            // Store current animated positions
            foreach (var bone in ragdollBones)
            {
                bone.storedPosition = bone.transform.position;
                bone.storedRotation = bone.transform.rotation;
            }
        
            // Disable animator
            if (animator != null)
                animator.enabled = false;
        
            // Enable ragdoll physics
            SetRagdollPhysics(true);
        
            _isRagdollActive = true;
            _isTransitioning = false;
        
            yield return null;
        }

        private IEnumerator TransitionFromRagdoll()
        {
            _isTransitioning = true;
        
            // Store ragdoll positions for blending
            foreach (var bone in ragdollBones)
            {
                bone.storedPosition = bone.transform.position;
                bone.storedRotation = bone.transform.rotation;
            }
        
            // Disable ragdoll physics
            SetRagdollPhysics(false);
        
            // Enable animator
            if (animator != null)
                animator.enabled = true;
        
            // Blend from ragdoll to animated pose
            float elapsedTime = 0f;
        
            while (elapsedTime < transitionDuration)
            {
                float t = elapsedTime / transitionDuration;
                t = Mathf.SmoothStep(0f, 1f, t); // Smooth transition curve
            
                for (int i = 0; i < ragdollBones.Length; i++)
                {
                    var bone = ragdollBones[i];
                
                    // Blend position and rotation
                    bone.transform.position = Vector3.Lerp(bone.storedPosition, bone.transform.position, t);
                    bone.transform.rotation = Quaternion.Lerp(bone.storedRotation, bone.transform.rotation, t);
                }
            
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        
            _isRagdollActive = false;
            _isTransitioning = false;
        }

        private void SetRagdollState(bool enable)
        {
            _isRagdollActive = enable;
        
          
            if (animator != null)
                animator.enabled = !enable;
        
            SetRagdollPhysics(enable);
        }

        private void SetRagdollPhysics(bool enable)
        {
            foreach (var bone in ragdollBones)
            {
                if (bone.rigidbody != null)
                {
                    bone.rigidbody.isKinematic = !enable;
                    bone.rigidbody.detectCollisions = true;
                }
            
                if (bone.collider != null)
                {
                    bone.collider.enabled = true;
                }
            }
        }

        // Apply explosion force to ragdoll
        public void AddExplosionForce(float force, Vector3 position, float radius)
        {
            if (!_isRagdollActive) EnableRagdoll(true);
        
            foreach (var bone in ragdollBones)
            {
                if (bone.rigidbody != null)
                {
                    bone.rigidbody.AddExplosionForce(force, position, radius);
                }
            }
        }

        // Apply directional force
        public void AddForce(Vector3 force, Vector3 position)
        {
            if (!_isRagdollActive) EnableRagdoll(true);
        
            // Find closest bone to impact point
            RagdollBone closestBone = GetClosestBone(position);
            if (closestBone?.rigidbody != null)
            {
                closestBone.rigidbody.AddForceAtPosition(force, position, ForceMode.Impulse);
            }
        }

        private RagdollBone GetClosestBone(Vector3 position)
        {
            RagdollBone closest = null;
            float closestDistance = float.MaxValue;
        
            foreach (var bone in ragdollBones)
            {
                float distance = Vector3.Distance(bone.transform.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = bone;
                }
            }
        
            return closest;
        }

        public bool IsRagdollActive => _isRagdollActive;
        public bool IsTransitioning => _isTransitioning;
    }
}