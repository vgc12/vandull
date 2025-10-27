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

        public float PlayAnimation(ItemAnimation itemAnimation)
        {
            if (itemAnimation == null) return 0f;

            animator.Play(itemAnimation.AnimationHash, 0);

            if (AnimationLengths.TryGetValue(itemAnimation.AnimationHash, out var length)) return length;

            length = animator.GetAnimationLength(itemAnimation.AnimationHash);
            AnimationLengths[itemAnimation.AnimationHash] = length;

            return length;
        }
    }
}