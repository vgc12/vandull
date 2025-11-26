using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public sealed class MandlebrotRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private MandlebrotRenderFeatureSettings settings;
    private MandelbrotRenderFeaturePass m_ScriptablePass;

    /// <inheritdoc />
    public override void Create()
    {
        m_ScriptablePass = new MandelbrotRenderFeaturePass(settings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.requiresIntermediateTexture = true;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }

    // Use this class to pass around settings from the feature to the pass
    [Serializable]
    public class MandlebrotRenderFeatureSettings
    {
        public Material mandelbrotMaterial;

        [Header("Mandelbrot Settings")] public float zoom = 1.0f;

        public Vector2 center = new(-0.5f, 0.0f);
        [Range(1, 500)] public int maxIterations = 100;

        [Header("Coloring")] public float colorMultiplier = 1.0f;

        public float colorOffset;
        public bool smoothColor = true;
        public Texture2D gradientTexture;

        [Header("Animation")] public bool animateZoom;

        public float zoomSpeed = 0.5f;
        public bool animatePan;
        public float panSpeed = 0.1f;
    }

    private class MandelbrotRenderFeaturePass : ScriptableRenderPass
    {
        private static readonly int ZoomID = Shader.PropertyToID("_Zoom");
        private static readonly int CenterXID = Shader.PropertyToID("_CenterX");
        private static readonly int CenterYID = Shader.PropertyToID("_CenterY");
        private static readonly int MaxIterationsID = Shader.PropertyToID("_MaxIterations");
        private static readonly int ColorMultiplierID = Shader.PropertyToID("_ColorMultiplier");
        private static readonly int ColorOffsetID = Shader.PropertyToID("_ColorOffset");
        private static readonly int ZoomSpeedID = Shader.PropertyToID("_ZoomSpeed");
        private static readonly int PanSpeedID = Shader.PropertyToID("_PanSpeed");
        private static readonly int GradientTexID = Shader.PropertyToID("_GradientTex");

        private readonly Material _material;
        private readonly MandlebrotRenderFeatureSettings settings;

        public MandelbrotRenderFeaturePass(MandlebrotRenderFeatureSettings settings)
        {
            this.settings = settings;
            _material = this.settings.mandelbrotMaterial;
        }


        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_material == null) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            if (!resourceData.isActiveTargetBackBuffer)
            {
                var source = resourceData.activeColorTexture;

                var desc = renderGraph.GetTextureDesc(source);
                desc.name = "_MandelbrotTempTexture";
                desc.clearBuffer = false;

                var destination = renderGraph.CreateTexture(desc);

                _material.SetFloat(ZoomID, settings.zoom);
                _material.SetFloat(CenterXID, settings.center.x);
                _material.SetFloat(CenterYID, settings.center.y);
                _material.SetInt(MaxIterationsID, settings.maxIterations);
                _material.SetFloat(ColorMultiplierID, settings.colorMultiplier);
                _material.SetFloat(ColorOffsetID, settings.colorOffset);
                _material.SetFloat(ZoomSpeedID, settings.zoomSpeed);
                _material.SetFloat(PanSpeedID, settings.panSpeed);

                RenderGraphUtils.BlitMaterialParameters para = new(destination, source, _material, 0);
                renderGraph.AddBlitPass(para, "Mandelbrot Blit Pass");

                RenderGraphUtils.BlitMaterialParameters paraCopy = new(destination, source, _material, 0);
                renderGraph.AddBlitPass(paraCopy, "Mandelbrot Copy Pass");
            }
        }
    }
}