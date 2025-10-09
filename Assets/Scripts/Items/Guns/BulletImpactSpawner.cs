using System.Collections;
using System.Collections.Generic;
using EventBus;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;

namespace Items.Guns
{
    public class BulletImpactSpawner : MonoBehaviour
    {
        [SerializeField] private Material decalMaterial;

        [SerializeField] private LayerMask decalLayers = -1;

        [SerializeField] private Vector3 decalSize = new(0.5f, 0.5f, 0.5f);

        [SerializeField] private float fadeDuration = 5f;

        [SerializeField] private float offset = .01f;

        [SerializeField] private List<Texture> decalTextures = new();


        private IObjectPool<DecalProjector> _decalPool;

        private EventBinding<ShotHitEvent> _shotHitEventBinding;

        public void Start()
        {
            _shotHitEventBinding = new EventBinding<ShotHitEvent>(SpawnDecal);

            EventBus<ShotHitEvent>.Register(_shotHitEventBinding);
            _decalPool = new ObjectPool<DecalProjector>(
                () =>
                {
                    var go = new GameObject("Decal Projector");
                    var dp = go.AddComponent<DecalProjector>();
                    dp.material = decalMaterial;
                    dp.material.mainTexture = decalTextures[Random.Range(0, decalTextures.Count)];
                    dp.fadeFactor = 1f;
                    dp.fadeScale = .95f;
                    dp.startAngleFade = 0;
                    dp.endAngleFade = 30;
                    dp.pivot = Vector3.zero;
                    return dp;
                },
                dp => dp.gameObject.SetActive(true),
                dp => dp.gameObject.SetActive(false),
                Destroy,
                false,
                100,
                300
            );
        }

        private void SpawnDecal(ShotHitEvent obj)
        {
            var decal = _decalPool.Get();
            decal.transform.position = obj.Hit.point + obj.Hit.normal * offset;
            var normalRotation = Quaternion.LookRotation(-obj.Hit.normal, Vector3.up);
            var randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            decal.transform.rotation = normalRotation * randomRotation;

            decal.size = decalSize;

            StartCoroutine(FadeAndRelease(decal, fadeDuration));
        }

        private IEnumerator FadeAndRelease(DecalProjector decal, float duration)
        {
            float time = 0;
            var initialFade = decal.fadeFactor;
            while (time < duration)
            {
                if (decal == null) yield break;

                time += Time.deltaTime;
                var t = time / duration;
                decal.fadeFactor = Mathf.Lerp(initialFade, 0f, t);
                yield return null;
            }

            if (decal != null)
            {
                decal.fadeFactor = initialFade;
                _decalPool.Release(decal);
            }
        }
        
        
        private void OnDestroy()
        {
            EventBus<ShotHitEvent>.Deregister(_shotHitEventBinding);
            _decalPool.Clear();
        }
    }
}