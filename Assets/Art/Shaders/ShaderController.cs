using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Singletons;
using UnityEngine;

namespace Art.Shaders
{
    public sealed class ShaderController : Singleton<ShaderController>
    {
        private static readonly int XRayAlpha = Shader.PropertyToID("_Alpha");
        private static readonly int XRayEnabled = Shader.PropertyToID("_XRayEnabled");


        public void ToggleXrayShaderOnObject(GameObject obj, bool enable)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            foreach (var mat in renderer.materials)
            {
                if (enable)
                {
                    mat.EnableKeyword("_XRayEnabled");
                }
                else
                {
                    mat.DisableKeyword("_XRayEnabled");
                }
            }
        }

        public async UniTask FadeXrayShader(GameObject obj, float targetIntensity, float duration,
            CancellationToken ct = default)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            var elapsed = 0f;


            var initalAlphas = new Dictionary<Material, float>();
            foreach (var renderer in renderers)
            foreach (var mat in renderer.materials)
            {
                if (!mat.HasProperty(XRayEnabled))
                {
                    continue;
                }

                mat.EnableKeyword("_XRayEnabled");

                initalAlphas[mat] = mat.GetFloat(XRayAlpha);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);

                foreach (var kvp in initalAlphas)
                {
                    var mat = kvp.Key;
                    var initialIntensity = kvp.Value;
                    var newIntensity = Mathf.Lerp(initialIntensity, targetIntensity, t);
                    mat.SetFloat(XRayAlpha, newIntensity);
                }

                await UniTask.Yield();
            }


            foreach (var kvp in initalAlphas)
            {
                var mat = kvp.Key;
                mat.SetFloat(XRayAlpha, targetIntensity);
            }
        }
    }
}