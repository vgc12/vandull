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

        public Transform leftHandTarget;
        public Transform leftHandHint;
        public Transform rightHandTarget;
        public Transform rightHandHint;

        [Header("Left Hand Following")] [SerializeField]
        private bool leftHandFollowItemTarget = true;

        [SerializeField] private bool leftHandFollowItemHint = true;

        [Header("Right Hand Following")] [SerializeField]
        private bool rightHandFollowItemTarget = true;

        [SerializeField] private bool rightHandFollowItemHint = true;


        public bool LeftHandFollowItemTarget
        {
            get => leftHandFollowItemTarget;
            set
            {
                RebuildRigs();
                leftHandFollowItemTarget = value;
            }
        }

        public bool LeftHandFollowItemHint
        {
            get => leftHandFollowItemHint;
            set
            {
                RebuildRigs();
                leftHandFollowItemHint = value;
            }
        }

        public bool RightHandFollowItemTarget
        {
            get => rightHandFollowItemTarget;
            set
            {
                RebuildRigs();
                rightHandFollowItemTarget = value;
            }
        }

        public bool RightHandFollowItemHint
        {
            get => rightHandFollowItemHint;
            set
            {
                RebuildRigs();
                rightHandFollowItemHint = value;
            }
        }


        private void Update()
        {
            if (leftHandConstraint && leftHandTarget && leftHandHint)
                ConstraintFollowTransform(
                    leftHandConstraint,
                    leftHandTarget,
                    leftHandHint,
                    LeftHandFollowItemTarget,
                    LeftHandFollowItemHint
                );


            if (rightHandConstraint && rightHandTarget && rightHandHint)
                ConstraintFollowTransform(
                    rightHandConstraint,
                    rightHandTarget,
                    rightHandHint,
                    RightHandFollowItemTarget,
                    RightHandFollowItemHint
                );
        }


        public async void SetLeftHandData(Transform leftHandTarget, Transform leftHandHint)
        {
            ApplyConstraint(leftHandConstraint, leftHandTarget, leftHandHint);
        }

        public async void SetRightHandData(Transform rightHandTarget, Transform rightHandHint)
        {
            ApplyConstraint(rightHandConstraint, rightHandTarget, rightHandHint);
        }


        public void ConstraintFollowTransform(
            TwoBoneIKConstraint constraint,
            Transform target,
            Transform hint,
            bool followTarget,
            bool followHint)
        {
            if (followTarget)
            {
                constraint.data.target.position = target.position;
                constraint.data.target.rotation = target.rotation;
            }

            if (followHint)
            {
                constraint.data.hint.position = hint.position;
                constraint.data.hint.rotation = hint.rotation;
            }
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