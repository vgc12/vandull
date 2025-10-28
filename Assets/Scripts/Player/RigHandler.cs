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

        public Transform LeftHandTarget { get; set; }
        public Transform LeftHandHint { get; set; }
        public Transform RightHandTarget { get; set; }
        public Transform RightHandHint { get; set; }

        public bool LeftHandFollowItemTarget { get; set; } = true;
        public bool RightHandFollowItemTarget { get; set; } = true;


        private void Update()
        {
            if (LeftHandFollowItemTarget && leftHandConstraint && LeftHandTarget && LeftHandHint)
                ConstraintFollowTransform(leftHandConstraint, LeftHandTarget, LeftHandHint);


            if (RightHandFollowItemTarget && rightHandConstraint && RightHandTarget && RightHandHint)
                ConstraintFollowTransform(rightHandConstraint, RightHandTarget, RightHandHint);
        }


        public void SetLeftHandData(Transform leftHandTarget, Transform leftHandHint)
        {
            ApplyConstraint(leftHandConstraint, leftHandTarget, leftHandHint);
        }

        public void SetRightHandData(Transform rightHandTarget, Transform rightHandHint)
        {
            ApplyConstraint(rightHandConstraint, rightHandTarget, rightHandHint);
        }


        public void ConstraintFollowTransform(TwoBoneIKConstraint constraint, Transform target, Transform hint)
        {
            constraint.data.target.position = target.position;
            constraint.data.target.rotation = target.rotation;
            constraint.data.hint.position = hint.position;
            constraint.data.hint.rotation = hint.rotation;
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

        public void RebuildRigs()
        {
            rigBuilder.Build();
        }
    }
}