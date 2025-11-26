using System;
using UnityEngine;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "New Grip Type", menuName = "Animation/Item Animation", order = 1)]
    public sealed class ItemAnimation : ScriptableObject
    {
        public string displayName;
        public string animationStateName;
        public int layer;

        [NonSerialized] private int? _animationHash;

        public int AnimationHash
        {
            get
            {
                _animationHash ??= Animator.StringToHash(animationStateName);
                return _animationHash.Value;
            }
        }
    }
}