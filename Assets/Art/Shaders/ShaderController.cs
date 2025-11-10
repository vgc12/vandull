using Singletons;
using UnityEngine;

namespace Art.Shaders
{
    public class ShaderController : Singleton<ShaderController>
    {
        protected override void Awake() { base.Awake(); }


        public void ToggleXrayShaderOnObject(GameObject obj, bool enable)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
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
        }
    }
}