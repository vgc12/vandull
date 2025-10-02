using Attributes;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RigHandler : MonoBehaviour
{
    [Required] public TwoBoneIKConstraint leftHandConstraint;
    public TwoBoneIKConstraint rightHandConstraint;


    public void SetLeftHandData(Transform leftHandTarget, Transform leftHandHint)
    {
        if (!leftHandConstraint) return;
        leftHandConstraint.data.target = leftHandTarget;
        leftHandConstraint.data.hint = leftHandHint;
    }

    public void SetRightHandData(Transform rightHandTarget, Transform rightHandHint)
    {
        if (!rightHandConstraint) return;
        rightHandConstraint.data.target = rightHandTarget;
        rightHandConstraint.data.hint = rightHandHint;
    }
}