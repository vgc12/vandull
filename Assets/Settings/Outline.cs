using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class OutlineRenderFeature : ScriptableRendererFeature
{
    public Settings settings = new();
    private OutlinePass _outlinePass;

    public override void Create()
    {
        _outlinePass = new OutlinePass(settings);
        _outlinePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.outlineMaterial == null) return;
        renderer.EnqueuePass(_outlinePass);
    }

    [Serializable]
    public class Settings
    {
        public Material outlineMaterial;
        public Color outlineColor = Color.black;
        public float outlineThickness = 1f;
        public float outlineThreshold = 0.01f;
        public bool posterize = true;
        public float posterizationCount = 8;
    }

    private class OutlinePass : ScriptableRenderPass
    {
        private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
        private static readonly int OutlineThresholdID = Shader.PropertyToID("_OutlineThreshold");
        private static readonly int PosterizeID = Shader.PropertyToID("_Posterize");
        private static readonly int PosterizationCountID = Shader.PropertyToID("_PosterizationCount");

        private readonly Material _material;
        private readonly Settings _settings;
        private RTHandle _tempTexture;

        public OutlinePass(Settings settings)
        {
            _settings = settings;
            _material = settings.outlineMaterial;
        }


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

                var destination = renderGraph.CreateTexture(desc);

                // Set shader properties
                _material.SetColor(OutlineColorID, _settings.outlineColor);
                _material.SetFloat(OutlineThicknessID, _settings.outlineThickness);
                _material.SetFloat(OutlineThresholdID, _settings.outlineThreshold);
                _material.SetFloat(PosterizeID, _settings.posterize ? 1 : 0);
                _material.SetFloat(PosterizationCountID, _settings.posterizationCount);
                // _material.SetFloat(NormalThresholdID, _settings.normalThreshold);

                // _material.SetFloat(DepthSensitivityID, _settings.depthSensitivity);

                // Add blit pass
                RenderGraphUtils.BlitMaterialParameters para = new(source, destination, _material, 0);
                renderGraph.AddBlitPass(para, "Outline Blit Pass");

                // Copy back
                RenderGraphUtils.BlitMaterialParameters paraCopy = new(destination, source, _material, 0);
                renderGraph.AddBlitPass(paraCopy, "Outline Copy Pass");
            }
        }


        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            _tempTexture?.Release();
        }
    }
}