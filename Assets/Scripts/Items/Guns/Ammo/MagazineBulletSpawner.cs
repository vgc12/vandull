using System.Collections.Generic;
using Art.Shaders;
using UnityEngine;
using UnityEngine.Pool;

namespace Items.Guns.Ammo
{
    public enum CurveMode { StraightLine, BezierCurve }

    public enum SpacingMode
    {
        FixedGap, // Bullets stack with a fixed distance between them
        SpreadAlongCurve // Bullets spread evenly across the entire curve
    }

    [ExecuteAlways]
    public class MagazineBulletSpawner : MonoBehaviour
    {
        [Header("Bullet Settings")] public GameObject bulletPrefab;

        public int bulletCount = 30;
        public Vector3 bulletRotationEuler = Vector3.zero;
        public bool followCurve = true;
        public Vector3 lastBulletPosition = Vector3.zero;
        public Vector3 lastBulletRotation = Vector3.zero;

        [Header("Bezier Curve Layout")] public Transform startPoint;

        public Transform endPoint;
        public Transform controlPoint1;
        public Transform controlPoint2;

        [Header("Modes")] public CurveMode curveMode = CurveMode.BezierCurve;

        public SpacingMode spacingMode = SpacingMode.SpreadAlongCurve;

        [Tooltip("Approximate gap between bullets when using Fixed Gap mode (curve parameter space)")]
        public float bulletGapParameter = 0.033f;

        [Header("Stacking")] public bool useStacking = true;

        public int bulletsPerStack = 2;
        public Vector3 stackOffset = new(0.005f, 0, 0);

        [Header("Edit Mode Preview")] public bool showInEditMode;

        private readonly List<GameObject> _spawnedBullets = new();

        // Instance-specific object pool
        private ObjectPool<GameObject> _bulletPool;

#if UNITY_EDITOR

        private void OnEnable()
        {
            if (Application.isPlaying || !showInEditMode)
            {
                return;
            }

            if (_spawnedBullets.Count == 0)
            {
                SpawnBullets();
            }
        }

#endif

        private void OnDisable() { ClearBullets(); }

        private void OnDrawGizmos()
        {
            // Draw bezier curve
            Gizmos.color = Color.yellow;
            var prevPos = transform.TransformPoint(startPoint.localPosition);
            for (var i = 20; i >= 1; i--)
            {
                var t = i / 20f;
                var oneMinusT = 1f - t;
                var pos = oneMinusT * oneMinusT * oneMinusT * startPoint.localPosition +
                          3f * oneMinusT * oneMinusT * t * controlPoint1.localPosition +
                          3f * oneMinusT * t * t * controlPoint2.localPosition +
                          t * t * t * endPoint.localPosition;
                var worldPos = transform.TransformPoint(pos);
                Gizmos.DrawLine(prevPos, worldPos);
                prevPos = worldPos;
            }

            // Draw control points
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(startPoint.position, 0.005f);
            Gizmos.DrawSphere(endPoint.position, 0.005f);
            Gizmos.color = Color.blue;
            if (curveMode == CurveMode.BezierCurve)
            {
                Gizmos.DrawSphere(controlPoint1.position, 0.004f);
                Gizmos.DrawSphere(controlPoint2.position, 0.004f);
                Gizmos.DrawLine(startPoint.position, controlPoint1.position);
                Gizmos.DrawLine(endPoint.position, controlPoint2.position);
            }
            else if (curveMode == CurveMode.StraightLine)
            {
                Gizmos.DrawLine(startPoint.position, endPoint.position);
            }

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
                    var tangent = GetTangent(i);
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
            UpdateSpacing();
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
                    () =>
                    {
                        var bullet = Instantiate(bulletPrefab);
                        bullet.SetActive(true);
                        return bullet;
                    },
                    obj => obj.SetActive(true),
                    obj => obj.SetActive(false),
                    Destroy,
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
                        ApplyLastBulletOffset();
                    }
                }
            }
        }

        private void ApplyLastBulletOffset()
        {
            _spawnedBullets[^1].transform.localPosition = lastBulletPosition;

            _spawnedBullets[^1].transform.localRotation = Quaternion.Euler(lastBulletRotation);
        }

        private float GetTForBullet(int i)
        {
            if (spacingMode == SpacingMode.FixedGap)
            {
                // Fixed gap: stack bullets with constant parameter spacing
                var t = i * bulletGapParameter;
                return Mathf.Clamp01(t); // Clamp to 0-1 range
            }

            // SpacingMode.SpreadAlongCurve
            // Spread evenly: distribute across the entire curve
            return (float)i / Mathf.Max(1, bulletCount - 1);
        }

        private Vector3 GetBezierTangent(int i)
        {
            var t = GetTForBullet(i);

            // Derivative of cubic Bezier curve: B'(t) = 3(1-t)^2(P1-P0) + 6(1-t)t(P2-P1) + 3t^2(P3-P2)
            var oneMinusT = 1f - t;
            var tangent = 3f * oneMinusT * oneMinusT * (controlPoint1.localPosition - startPoint.localPosition) +
                          6f * oneMinusT * t * (controlPoint2.localPosition - controlPoint1.localPosition) +
                          3f * t * t * (endPoint.localPosition - controlPoint2.localPosition);

            return tangent;
        }


        private Vector3 GetTangent(int i) =>
            curveMode == CurveMode.BezierCurve ? GetBezierTangent(i) : endPoint.localPosition - startPoint.localPosition;

        private Quaternion GetBulletRotation(int i)
        {
            if (!followCurve)
            {
                return Quaternion.Euler(bulletRotationEuler);
            }

            var tangent = GetTangent(i);

            if (tangent.sqrMagnitude < 0.0001f)
            {
                return Quaternion.Euler(bulletRotationEuler);
            }

            // Create rotation that points the bullet along the tangent
            var rotation = Quaternion.LookRotation(tangent.normalized);

            // Apply additional rotation offset
            rotation *= Quaternion.Euler(bulletRotationEuler);

            return rotation;
        }

        private Vector3 GetBulletPositionFromBezier(int i)
        {
            var t = GetTForBullet(i);

            // Cubic Bezier curve: B(t) = (1-t)^3P0 + 3(1-t)^2tP1 + 3(1-t)t^2P2 + t^3P3
            var oneMinusT = 1f - t;
            var pos = oneMinusT * oneMinusT * oneMinusT * startPoint.localPosition +
                      3f * oneMinusT * oneMinusT * t * controlPoint1.localPosition +
                      3f * oneMinusT * t * t * controlPoint2.localPosition +
                      t * t * t * endPoint.localPosition;

            return pos;
        }

        private Vector3 GetBulletPosition(int i)
        {
            var pos = curveMode == CurveMode.BezierCurve
                ? GetBulletPositionFromBezier(i)
                : Vector3.Lerp(startPoint.localPosition, endPoint.localPosition, GetTForBullet(i));

            if (useStacking)
            {
                var stackLayer = i % bulletsPerStack;
                pos += stackOffset * stackLayer;
            }

            return pos;
        }

        [ContextMenu("Spawn Bullets")]
        public void SpawnBullets(int bulletCountOverride = -1)
        {
            var countToSpawn = bulletCountOverride > 0 ? bulletCountOverride : bulletCount - 1;


            ClearBullets();

            for (var i = countToSpawn; i >= 0; i--)
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


                bullet.transform.localPosition = position;
                bullet.transform.localRotation = GetBulletRotation(i);

                _spawnedBullets.Add(bullet);
            }

            _spawnedBullets.ForEach(b => b.SetActive(true));
            if (lastBulletOffset)
            {
                ApplyLastBulletOffset();
            }
        }

        public bool lastBulletOffset;


        public void ReleaseAllBulletsToPool()
        {
            var pool = GetOrCreatePool();
            if (pool == null)
            {
                return;
            }

            foreach (var bullet in _spawnedBullets)
            {
                if (bullet != null)
                {
                    bullet.SetActive(false);
                    pool.Release(bullet);
                }
            }

            _spawnedBullets.Clear();
        }

        public void ClearBullets()
        {
            if (Application.isPlaying)
            {
                ReleaseAllBulletsToPool();
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
            ShaderController.Instance.ToggleXrayShaderOnObject(gameObject, visible);
        }
    }
}