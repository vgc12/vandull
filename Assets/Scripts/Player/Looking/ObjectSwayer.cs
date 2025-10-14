using Attributes;
using UnityEngine;

namespace Player.Looking
{
    public class ObjectSwayer : MonoBehaviour
    {
        [Header("Transform References")] [SerializeField] [Required]
        private Transform swayedObjectTransform;

        [SerializeField] private float swayLerpSpeed = 5f;


        private float _swayTimer;

        private void Update()
        {
            _swayTimer += Time.deltaTime;
        }


        public void Sway(SwayConfig sway)
        {
            var swayPosition = CalculateSwayPosition(sway);
            swayedObjectTransform.localPosition = Vector3.Lerp(
                swayedObjectTransform.localPosition,
                swayPosition,
                Time.deltaTime * swayLerpSpeed
            );
        }

        private Vector3 CalculateSwayPosition(SwayConfig sway)
        {
            var horizontalSway = Mathf.Cos(_swayTimer * sway.horizontalSwaySpeed) *
                                 sway.horizontalSwayAmount * sway.swayMultiplier;

            var verticalSway = Mathf.Sin(_swayTimer * sway.verticalSwaySpeed) *
                               sway.verticalSwayAmount * sway.swayMultiplier;

            return new Vector3(
                horizontalSway,
                verticalSway,
                swayedObjectTransform.localPosition.z
            );
        }
    }
}