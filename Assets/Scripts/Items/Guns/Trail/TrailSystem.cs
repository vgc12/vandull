using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Items.Guns.Trail
{
    public class TrailSystem : ITrailSystem
    {
        private readonly ObjectPool<TrailRenderer> _trailPool;
        
        private readonly TrailConfig _trailConfig;
        
        public TrailSystem( TrailConfig trailConfig )
        {
            _trailConfig = trailConfig;
            _trailPool = new ObjectPool<TrailRenderer>(CreateTrail);
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
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            
            return trail;
        }
        
        public IEnumerator SpawnTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
        {
             var instance = _trailPool.Get();
            instance.gameObject.SetActive(true);
            instance.transform.position = startPoint;
            yield return null; 

            instance.emitting = true;

            var distance = Vector3.Distance(startPoint, endPoint);
            var remainingDistance = distance;
            while (remainingDistance > 0)
            {
                instance.transform.position = Vector3.Lerp(
                    startPoint,
                    endPoint,
                    Mathf.Clamp01(1 - (remainingDistance / distance))
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
            instance.gameObject.SetActive(false);
            _trailPool.Release(instance);
//Debug.DrawLine(startPoint, endPoint, Color.blue, 1000f);
/*
            if (BulletPenConfig != null && BulletPenConfig.MaxObjectsToPenetrate > Iteration)
            {
                yield return null;
                Vector3 direction = (EndPoint - StartPoint).normalized;
                Vector3 backCastOrigin = Hit.point + direction * BulletPenConfig.MaxPenetrationDepth;

                if (Physics.Raycast(
                        backCastOrigin,
                        -direction,
                        out RaycastHit hit,
                        BulletPenConfig.MaxPenetrationDepth,
                        ShootConfig.HitMask
                    ))
                {
                    Vector3 penetrationOrigin = hit.point;
                    direction += new Vector3(
                        Random.Range(-BulletPenConfig.AccuracyLoss.x, BulletPenConfig.AccuracyLoss.x),
                        Random.Range(-BulletPenConfig.AccuracyLoss.y, BulletPenConfig.AccuracyLoss.y),
                        Random.Range(-BulletPenConfig.AccuracyLoss.z, BulletPenConfig.AccuracyLoss.z)
                    );

                    DoHitscanShoot(direction, penetrationOrigin, penetrationOrigin, Iteration + 1);
                }
            }
        
        
*/
        }
    }
    
    
    public interface ITrailSystem
    {
        
        IEnumerator SpawnTrail(Vector3 start, Vector3 end, RaycastHit hit);
    }
}