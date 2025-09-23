using System;
using UnityEngine;

namespace NPC
{
    public class AnimationController : MonoBehaviour
    {
        
        public int HorizontalMovement { get; set; } = UnityEngine.Animator.StringToHash("HorizontalMovement");
        public int VerticalMovement { get; set; } = UnityEngine.Animator.StringToHash("VerticalMovement");
        
        public Animator Animator { get; private set; }

        private void Awake()
        {
            Animator = GetComponent<Animator>();
        }

        public void HandleMovementBlendTree(Vector3 velocity)
        {
            Vector3 vel = velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(vel).normalized;
            float speed = vel.magnitude;


            if (speed > 0.01f)
            {
                Animator.SetFloat(HorizontalMovement, localVelocity.x / 2);
                Animator.SetFloat(VerticalMovement, localVelocity.z / 2);
            }
            else
            {
                Animator.SetFloat(HorizontalMovement, 0);
                Animator.SetFloat(VerticalMovement, 0);
            }
        }
    }
}