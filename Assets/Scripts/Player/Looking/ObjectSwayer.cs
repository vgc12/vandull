using Attributes;
using UnityEngine;

namespace Player.Looking
{
    public class ObjectSwayer : MonoBehaviour
    {
        [Header("Configuration")]
        [Required, ScriptableObjectDropdown] 
        public SwayConfig swayConfig;

        [Header("Transform References")]
        [SerializeField, Required] 
        private Transform swayedObjectTransform;

   
        private float _swayTimer;
        
        [SerializeField] private  float swayLerpSpeed = 5f;
        

        public void Sway(SwayConfig sway)
        {
            var swayPosition = CalculateSwayPosition(sway);
            swayedObjectTransform.localPosition = Vector3.Lerp(
                swayedObjectTransform.localPosition, 
                swayPosition, 
                Time.deltaTime * swayLerpSpeed
            );
        }

        private void Update()
        {
            _swayTimer += Time.deltaTime;
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