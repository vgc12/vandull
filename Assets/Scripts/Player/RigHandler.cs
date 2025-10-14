using Attributes;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Player
{
    public class RigHandler : MonoBehaviour
    {
        [Required] public TwoBoneIKConstraint leftHandConstraint;
        public TwoBoneIKConstraint rightHandConstraint;

        public RigBuilder rigBuilder;


        public void SetLeftHandData(Transform leftHandTarget, Transform leftHandHint)
        {
            ApplyConstraint(leftHandConstraint, leftHandTarget, leftHandHint);
        }

        public void SetRightHandData(Transform rightHandTarget, Transform rightHandHint)
        {
            ApplyConstraint(rightHandConstraint, rightHandTarget, rightHandHint);
        }


        private void ApplyConstraint(TwoBoneIKConstraint constraint, Transform target, Transform hint)
        {
            if (constraint == null || target == null || hint == null) return;

            var data = constraint.data;
            data.target = target;
            data.hint = hint;

            constraint.data = data;
            RebuildRigs();
        }

        private void RebuildRigs()
        {
            rigBuilder.Build();
        }
    }
}