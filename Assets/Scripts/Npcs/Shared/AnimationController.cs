using UnityEngine;

namespace Npcs.Shared
{
    public sealed class AnimationController : MonoBehaviour
    {
        public int HorizontalMovement { get; set; } = Animator.StringToHash("HorizontalMovement");
        public int VerticalMovement { get; set; } = Animator.StringToHash("VerticalMovement");

        public Animator Animator { get; private set; }

        private void Awake()
        {
            Animator = GetComponent<Animator>();
        }

        public void HandleMovementBlendTree(Vector3 velocity)
        {
            var vel = velocity;
            var localVelocity = transform.InverseTransformDirection(vel).normalized;
            var speed = vel.magnitude;


            if (speed > 0.01f)
            {
                Animator.SetFloat(HorizontalMovement, localVelocity.x * .6f);
                Animator.SetFloat(VerticalMovement, localVelocity.z * .6f);
            }
            else
            {
                Animator.SetFloat(HorizontalMovement, 0);
                Animator.SetFloat(VerticalMovement, 0);
            }
        }
    }
}