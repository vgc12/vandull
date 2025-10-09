using System.Collections;
using EventBus;
using UnityEngine;
using UnityEngine.Rendering;

namespace Items.Guns.Trail
{
    public class TrailSystem : ITrailSystem
    {
        private readonly UnityEngine.Pool.ObjectPool<TrailRenderer> _trailPool;
        private readonly TrailSettings _trailSettings;

        public TrailSystem(TrailSettings trailSettings)
        {
            _trailSettings = trailSettings;
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
                if (_trailSettings.fadeOut)
                {
                    var fadeFactor = Mathf.Clamp01(remainingDistance / distance);

                    var fadedGradient = ApplyAlphaToGradient(originalGradient, fadeFactor);
                    instance.colorGradient = fadedGradient;
                }

                instance.transform.position = Vector3.Lerp(
                    startPoint,
                    endPoint,
                    Mathf.Clamp01(1 - remainingDistance / distance)
                );

                remainingDistance -= _trailSettings.simulationSpeed * Time.deltaTime;

                yield return null;
            }

            instance.transform.position = endPoint;

            if (hit.collider) EventBus<ShotHitEvent>.Raise(new ShotHitEvent(hit));

            yield return new WaitForSeconds(_trailSettings.duration);
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
            trail.colorGradient = _trailSettings.color;
            trail.material = _trailSettings.material;
            trail.widthCurve = _trailSettings.widthCurve;
            trail.time = _trailSettings.duration;
            trail.minVertexDistance = _trailSettings.minVertexDistance;

            trail.emitting = false;
            trail.shadowCastingMode = ShadowCastingMode.Off;

            return trail;
        }

        private Gradient CloneGradient(Gradient original)
        {
            var cloned = new Gradient();
            cloned.SetKeys(original.colorKeys, original.alphaKeys);
            cloned.mode = original.mode;
            return cloned;
        }


        private Gradient ApplyAlphaToGradient(Gradient original, float alphaMultiplier)
        {
            var modified = new Gradient();


            var colorKeys = original.colorKeys;

            var alphaKeys = original.alphaKeys;
            var newAlphaKeys = new GradientAlphaKey[alphaKeys.Length];

            for (var i = 0; i < alphaKeys.Length; i++)
                newAlphaKeys[i] = new GradientAlphaKey(
                    alphaKeys[i].alpha * alphaMultiplier,
                    alphaKeys[i].time
                );

            modified.SetKeys(colorKeys, newAlphaKeys);
            modified.mode = original.mode;

            return modified;
        }
    }
}