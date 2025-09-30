using UnityEngine;

namespace Player
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] [Range(0.1f, 5f)] private float groundCheckRadius = 0.3f;
        [SerializeField] private Vector3 groundCheckOffset = new(0, .4f, 0);
        [SerializeField] private LayerMask excludedLayers;
        [SerializeField] private bool debugMode;
        private readonly Collider[] _groundedColliders = new Collider[8];
        public bool IsGrounded { get; private set; }

        private void Update()
        {
            CheckForGround();
        }

        private void OnDrawGizmos()
        {
            if (!debugMode) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
        }

        private void CheckForGround()
        {
            IsGrounded =
                Physics.OverlapSphereNonAlloc(transform.position + groundCheckOffset, groundCheckRadius,
                    _groundedColliders, ~excludedLayers) >
                0;
        }
    }
}