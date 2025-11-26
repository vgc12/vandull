using System.Collections.Generic;
using General.Extensions;
using Items.Guns;
using Singletons;
using UnityEngine;

namespace Npcs.Shared
{
    public sealed class ArmAnimationController : Singleton<ArmAnimationController>
    {
        // Static cache shared across all instances (perfect for singletons)
        private static readonly Dictionary<int, float> AnimationLengths = new();
        private static readonly Dictionary<int, float> AnimationPlaybacks = new();
        [SerializeField] private Animator animator;

        public float PlayAnimation(ItemAnimation itemAnimation, float startTime = 0f)
        {
            if (!itemAnimation) return 0f;
            AnimationLengths.TryGetValue(itemAnimation.AnimationHash, out var length);

            if (length <= 0f)
            {
                length = animator.GetAnimationLength(itemAnimation.AnimationHash, itemAnimation.layer);
                AnimationLengths[itemAnimation.AnimationHash] = length;
            }

            animator.SetLayerWeight(itemAnimation.layer, 1f);
            startTime /= length;
            animator.Play(itemAnimation.AnimationHash, itemAnimation.layer, startTime);
            return length;
        }


        public float CrossFadeToAnimation(ItemAnimation itemAnimation, float duration, float startTime = 0)
        {
            if (!itemAnimation) return 0f;
            AnimationLengths.TryGetValue(itemAnimation.AnimationHash, out var length);

            if (length <= 0f)
            {
                length = animator.GetAnimationLength(itemAnimation.AnimationHash);
                AnimationLengths[itemAnimation.AnimationHash] = length;
            }

            animator.CrossFadeInFixedTime(itemAnimation.AnimationHash, duration, 0, startTime);
            return length;
        }


        public float GetCurrentAnimationTime()
        {
            var currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return currentAnimatorStateInfo.normalizedTime;
        }
    }
}