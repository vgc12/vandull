using System.Collections.Generic;
using Art.Shaders;
using UnityEngine;
using UnityEngine.Pool;

namespace Items.Guns.Ammo
{
    public enum CurveMode { AnimationCurves, BezierCurve }

    [ExecuteAlways]
    public class MagazineBulletSpawner : MonoBehaviour
    {
        [Header("Bullet Settings")] public GameObject bulletPrefab;

        public int bulletCount = 30;

        public Vector3 bulletRotationEuler = Vector3.zero;
        public bool followCurve = true;
        public Vector3 lastBulletPosition = Vector3.zero;

        [Header("Bezier Curve Layout")] public Vector3 startPoint = Vector3.zero;

        public Vector3 endPoint = new(0, 0.3f, 0.3f);
        public Vector3 controlPoint1 = new(0, 0.1f, 0.1f);
        public Vector3 controlPoint2 = new(0, 0.2f, 0.2f);

        [Header("Stacking")] public bool useStacking = true;

        public int bulletsPerStack = 2;
        public Vector3 stackOffset = new(0.005f, 0, 0);

        [Header("Edit Mode Preview")] public bool showInEditMode;

        private readonly List<GameObject> _spawnedBullets = new();

        // Instance-specific object pool
        private ObjectPool<GameObject> _bulletPool;

        private void Update()
        {
            if (Application.isPlaying)
            {
                if (_spawnedBullets.Count == 0)
                {
                    SpawnBullets();
                }

                UpdateSpacing();
            }
        }

        private void OnEnable() { SpawnBullets(); }

        private void OnDisable()
        {
            // Return bullets to pool when disabled
            if (Application.isPlaying)
            {
                ReturnBulletsToPool();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize bullet positions in editor
            Gizmos.color = Color.red;


            // Draw bezier curve
            Gizmos.color = Color.yellow;
            var prevPos = transform.TransformPoint(startPoint);
            for (var i = 1; i <= 20; i++)
            {
                var t = i / 20f;
                var oneMinusT = 1f - t;
                var pos = oneMinusT * oneMinusT * oneMinusT * startPoint +
                          3f * oneMinusT * oneMinusT * t * controlPoint1 +
                          3f * oneMinusT * t * t * controlPoint2 +
                          t * t * t * endPoint;
                var worldPos = transform.TransformPoint(pos);
                Gizmos.DrawLine(prevPos, worldPos);
                prevPos = worldPos;
            }

            // Draw control points
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(transform.TransformPoint(startPoint), 0.005f);
            Gizmos.DrawSphere(transform.TransformPoint(endPoint), 0.005f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.TransformPoint(controlPoint1), 0.004f);
            Gizmos.DrawSphere(transform.TransformPoint(controlPoint2), 0.004f);
            Gizmos.DrawLine(transform.TransformPoint(startPoint), transform.TransformPoint(controlPoint1));
            Gizmos.DrawLine(transform.TransformPoint(endPoint), transform.TransformPoint(controlPoint2));


            // Draw bullet positions
            Gizmos.color = Color.red;
            for (var i = 0; i < bulletCount; i++)
            {
                var pos = GetBulletPosition(i);
                Gizmos.DrawSphere(transform.TransformPoint(pos), 0.002f);

                // Draw tangent direction
                if (followCurve)
                {
                    Gizmos.color = Color.green;
                    var tangent = GetBezierTangent(i);
                    var worldPos = transform.TransformPoint(pos);
                    var worldTangent = transform.TransformDirection(tangent.normalized * 0.01f);
                    Gizmos.DrawLine(worldPos, worldPos + worldTangent);
                    Gizmos.color = Color.red;
                }
            }
        }

        private void OnValidate()
        {
            // Update bullets when values change in edit mode
            if (!Application.isPlaying && showInEditMode)
            {
                UpdateSpacing();
            }
        }

        private ObjectPool<GameObject> GetOrCreatePool()
        {
            if (bulletPrefab == null)
            {
                return null;
            }

            if (_bulletPool == null)
            {
                _bulletPool = new ObjectPool<GameObject>(
                    () => Instantiate(bulletPrefab),
                    obj => obj.SetActive(true),
                    obj => obj.SetActive(false),
                    obj => Destroy(obj),
                    true,
                    30,
                    100
                );
            }

            return _bulletPool;
        }

        private void UpdateSpacing()
        {
            for (var i = 0; i < _spawnedBullets.Count; i++)
            {
                if (_spawnedBullets[i] != null)
                {
                    _spawnedBullets[i].transform.localPosition = GetBulletPosition(i);
                    _spawnedBullets[i].transform.localRotation = GetBulletRotation(i);
                    if (i == _spawnedBullets.Count - 1)
                    {
                        _spawnedBullets[i].transform.localPosition = lastBulletPosition;
                    }
                }
            }
        }

        private Vector3 GetBezierTangent(int i)
        {
            var t = (float)i / Mathf.Max(1, bulletCount - 1);

            // Derivative of cubic Bezier curve: B'(t) = 3(1-t)^2(P1-P0) + 6(1-t)t(P2-P1) + 3t^2(P3-P2)
            var oneMinusT = 1f - t;
            var tangent = 3f * oneMinusT * oneMinusT * (controlPoint1 - startPoint) +
                          6f * oneMinusT * t * (controlPoint2 - controlPoint1) +
                          3f * t * t * (endPoint - controlPoint2);

            return tangent;
        }

        private Quaternion GetBulletRotation(int i)
        {
            if (!followCurve)
            {
                return Quaternion.Euler(bulletRotationEuler);
            }

            var tangent = GetBezierTangent(i);

            if (tangent.sqrMagnitude < 0.0001f)
            {
                return Quaternion.Euler(bulletRotationEuler);
            }

            // Create rotation that points the bullet along the tangent
            // Assuming the bullet's forward axis should align with the curve
            var rotation = Quaternion.LookRotation(tangent.normalized);

            // Apply additional rotation offset
            rotation *= Quaternion.Euler(bulletRotationEuler);

            return rotation;
        }

        private Vector3 GetBulletPositionFromBezier(int i)
        {
            var t = (float)i / Mathf.Max(1, bulletCount - 1);

            // Cubic Bezier curve: B(t) = (1-t)^3P0 + 3(1-t)^2tP1 + 3(1-t)t^2P2 + t^3P3
            var oneMinusT = 1f - t;
            var pos = oneMinusT * oneMinusT * oneMinusT * startPoint +
                      3f * oneMinusT * oneMinusT * t * controlPoint1 +
                      3f * oneMinusT * t * t * controlPoint2 +
                      t * t * t * endPoint;

            return pos;
        }

        private Vector3 GetBulletPosition(int i)
        {
            var pos = GetBulletPositionFromBezier(i);

            if (useStacking)
            {
                var stackLayer = i % bulletsPerStack;
                pos += stackOffset * stackLayer;
            }

            return pos;
        }

        [ContextMenu("Spawn Bullets")]
        public void SpawnBullets()
        {
            ClearBullets();

            for (var i = 0; i < bulletCount; i++)
            {
                var position = GetBulletPosition(i);
                GameObject bullet;

                if (Application.isPlaying)
                {
                    // Use object pool in play mode
                    var pool = GetOrCreatePool();
                    if (pool == null)
                    {
                        continue;
                    }

                    bullet = pool.Get();
                    bullet.transform.SetParent(transform);
                }
                else
                {
                    // Direct instantiation in edit mode
                    bullet = Instantiate(bulletPrefab, transform);
                    bullet.hideFlags = HideFlags.DontSave;
                }

                bullet.layer = gameObject.layer;
                bullet.transform.localPosition = position;
                bullet.transform.localRotation = GetBulletRotation(i);
                _spawnedBullets.Add(bullet);
            }
        }

        private void ReturnBulletsToPool()
        {
            var pool = GetOrCreatePool();
            if (pool == null)
            {
                return;
            }

            foreach (var bullet in _spawnedBullets)
            {
                if (bullet)
                {
                    pool.Release(bullet);
                }
            }

            _spawnedBullets.Clear();
        }

        [ContextMenu("Clear Bullets")]
        public void ClearBullets()
        {
            if (Application.isPlaying)
            {
                ReturnBulletsToPool();
            }
            else
            {
                // Edit mode: destroy immediately
                foreach (var bullet in _spawnedBullets)
                {
                    if (bullet != null)
                    {
                        DestroyImmediate(bullet);
                    }
                }

                _spawnedBullets.Clear();
            }
        }

        [ContextMenu("Clear Pool")]
        public void ClearPool()
        {
            if (_bulletPool != null)
            {
                _bulletPool.Clear();
                _bulletPool = null;
            }
        }

        public void ToggleXRayVisibility(bool visible)
        {
            // Implementation for X-Ray visibility toggle if needed
            ShaderController.Instance.ToggleXrayShaderOnObject(gameObject, visible);
        }
    }
}