using System.Collections.Generic;
using General.Extensions;
using Items.Guns;
using Singletons;
using UnityEngine;

namespace Npcs.Shared
{
    public class ArmAnimationController : Singleton<ArmAnimationController>
    {
        // Static cache shared across all instances (perfect for singletons)
        private static readonly Dictionary<int, float> AnimationLengths = new();
        [SerializeField] private Animator animator;

        public float PlayAnimation(ItemAnimation itemAnimation, float startTime = 0f)
        {
            
            if (!itemAnimation) return 0f;
            AnimationLengths.TryGetValue(itemAnimation.AnimationHash, out var length);
            
            if (length <= 0f)
            {
                length = animator.GetAnimationLength(itemAnimation.AnimationHash);
                AnimationLengths[itemAnimation.AnimationHash] = length;
            }
            
            animator.Play(itemAnimation.AnimationHash, 0, startTime/length);
    
        

            return length;
        }

        public float GetCurrentAnimationTime()
        {
            var currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return currentAnimatorStateInfo.normalizedTime * currentAnimatorStateInfo.length;
        }
    }
}