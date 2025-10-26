using Items.Guns;
using UnityEngine;

namespace Npcs.Shared
{
    public class ArmAnimationController : MonoBehaviour
    {
        public Animator animator;
        private readonly int _arNoGrip = Animator.StringToHash("AR_No_Grip");
        private readonly int _pistolGrip = Animator.StringToHash("Pistol_Grip");


        public void PlayAnimation(GripType type)
        {
            if (type == GripType.Pistol)
                animator.Play(_pistolGrip, 0);
            else
                animator.Play(_arNoGrip, 0);
        }
    }
}