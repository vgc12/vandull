using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Items.Guns.Trail
{
    public class TrailSystem : ITrailSystem
    {
        private readonly TrailConfig _trailConfig;
        private readonly UnityEngine.Pool.ObjectPool<TrailRenderer> _trailPool;

        public TrailSystem(TrailConfig trailConfig)
        {
            _trailConfig = trailConfig;
            _trailPool = new UnityEngine.Pool.ObjectPool<TrailRenderer>(CreateTrail);
        }

        public IEnumerator SpawnTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
        {
            var instance = _trailPool.Get();

            instance.gameObject.SetActive(true);
            instance.Clear();
            instance.transform.position = startPoint;
         
            yield return null;
            var originalGradient = CloneGradient(instance.colorGradient);
            
            instance.emitting = true;

            var distance = Vector3.Distance(startPoint, endPoint);
            var remainingDistance = distance;

            while (remainingDistance > 0)
            {
                if (_trailConfig.fadeOut)
                {
                  
                    float fadeFactor = Mathf.Clamp01(remainingDistance / distance);
                  
                    Gradient fadedGradient = ApplyAlphaToGradient(originalGradient, fadeFactor);
                    instance.colorGradient = fadedGradient;
                }
                
                instance.transform.position = Vector3.Lerp(
                    startPoint,
                    endPoint,
                    Mathf.Clamp01(1 - remainingDistance / distance)
                );

                remainingDistance -= _trailConfig.simulationSpeed * Time.deltaTime;

                yield return null;
            }

            instance.transform.position = endPoint;

            if (hit.collider)
            {
                //HandleBulletImpact(distance, EndPoint, Hit.normal, Hit.collider, Iteration);
            }

            yield return new WaitForSeconds(_trailConfig.duration);
            yield return null;
            instance.emitting = false;
            instance.colorGradient = originalGradient; 
            instance.gameObject.SetActive(false);
            _trailPool.Release(instance);
        }

        public void Update()
        {
        }

        private TrailRenderer CreateTrail()
        {
            var instance = new GameObject("BulletTrail");
            var trail = instance.AddComponent<TrailRenderer>();
            trail.colorGradient = _trailConfig.color;
            trail.material = _trailConfig.material;
            trail.widthCurve = _trailConfig.widthCurve;
            trail.time = _trailConfig.duration;
            trail.minVertexDistance = _trailConfig.minVertexDistance;

            trail.emitting = false;
            trail.shadowCastingMode = ShadowCastingMode.Off;

            return trail;
        }

        private Gradient CloneGradient(Gradient original)
        {
            Gradient cloned = new Gradient();
            cloned.SetKeys(original.colorKeys, original.alphaKeys);
            cloned.mode = original.mode;
            return cloned;
        }


        private Gradient ApplyAlphaToGradient(Gradient original, float alphaMultiplier)
        {
            Gradient modified = new Gradient();
            
   
            GradientColorKey[] colorKeys = original.colorKeys;
     
            GradientAlphaKey[] alphaKeys = original.alphaKeys;
            GradientAlphaKey[] newAlphaKeys = new GradientAlphaKey[alphaKeys.Length];
            
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                newAlphaKeys[i] = new GradientAlphaKey(
                    alphaKeys[i].alpha * alphaMultiplier,
                    alphaKeys[i].time
                );
            }
            
            modified.SetKeys(colorKeys, newAlphaKeys);
            modified.mode = original.mode;
            
            return modified;
        }
    }
}