using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class OutlineRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material outlineMaterial;
        public Color outlineColor = Color.black;
        public float outlineThickness = 1f;
        public float depthThreshold = 0.01f;
        public float normalThreshold = 0.5f;
        
    }

    public Settings settings = new Settings();
    OutlinePass _outlinePass;

    public override void Create()
    {
        _outlinePass = new OutlinePass(settings);
        _outlinePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null) return;
        renderer.EnqueuePass(_outlinePass);
    }

    class OutlinePass : ScriptableRenderPass
    {
        private readonly Settings _settings;
        private readonly Material _material;
        private RTHandle _tempTexture;
        
        // Properties for shader
        private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
        private static readonly int DepthThresholdID = Shader.PropertyToID("_DepthThreshold");
        private static readonly int NormalThresholdID = Shader.PropertyToID("_NormalThreshold");

        public OutlinePass(Settings settings)
        {
            this._settings = settings;
            this._material = settings.outlineMaterial;
        }

        // RenderGraph API (New method - required for newer URP versions)
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_material == null) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            // Check if camera color is valid
            if (!resourceData.isActiveTargetBackBuffer)
            {
                var source = resourceData.activeColorTexture;
                
                // Create temporary texture descriptor
                var desc = renderGraph.GetTextureDesc(source);
                desc.name = "_OutlineTempTexture";
                desc.clearBuffer = false;
                
                TextureHandle destination = renderGraph.CreateTexture(desc);

                // Set shader properties
                _material.SetColor(OutlineColorID, _settings.outlineColor);
                _material.SetFloat(OutlineThicknessID, _settings.outlineThickness);
                _material.SetFloat(DepthThresholdID, _settings.depthThreshold);
                _material.SetFloat(NormalThresholdID, _settings.normalThreshold);

                // Add blit pass
                RenderGraphUtils.BlitMaterialParameters para = new(source, destination, _material, 0);
                renderGraph.AddBlitPass(para, passName: "Outline Blit Pass");
                
                // Copy back
                RenderGraphUtils.BlitMaterialParameters paraCopy = new(destination, source, _material, 0);
                renderGraph.AddBlitPass(paraCopy, passName: "Outline Copy Pass");
            }
        }
/*
        // Legacy API (for compatibility mode or older URP versions)
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_OutlineTempTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("Outline Pass");
            
            // Set shader properties
            material.SetColor(OutlineColorID, settings.outlineColor);
            material.SetFloat(OutlineThicknessID, settings.outlineThickness);
            material.SetFloat(DepthSensitivityID, settings.depthSensitivity);
            material.SetFloat(NormalSensitivityID, settings.normalSensitivity);

            RTHandle cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            Blitter.BlitCameraTexture(cmd, cameraTarget, tempTexture, material, 0);
            Blitter.BlitCameraTexture(cmd, tempTexture, cameraTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
*/
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            _tempTexture?.Release();
        }

   
    }
}