using Items.Guns;
using UnityEngine;

namespace Npcs.Shared
{
    public class ArmAnimationController : MonoBehaviour
    {
        private readonly int _arNoGrip = Animator.StringToHash("AR_No_Grip");
        private readonly int _pistolGrip = Animator.StringToHash("Pistol_Grip");
        public Animator animator;

        public void PlayAnimation(GripType type)
        {
            if(type == GripType.Pistol)
              animator.CrossFade(_pistolGrip, 0);
            else
              animator.CrossFade(_arNoGrip, 0);
        }
    }
}