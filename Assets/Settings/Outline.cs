using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Settings
{
    public sealed class OutlineRenderFeature : ScriptableRendererFeature
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
            public float depthThreshold = 0.1f;
            public float normalThreshold = 0.4f;
            public bool posterize = true;
            public float posterizationCount = 8;
            public bool useNormals = true;
            public bool useCanny;
        }

        private class OutlinePass : ScriptableRenderPass
        {
            private const string USE_NORMALS_KEYWORD = "USE_NORMALS";
            private const string USE_CANNY_KEYWORD = "USE_CANNY";
            private const string POSTERIZE_KEYWORD = "POSTERIZE";
            private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
            private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
            private static readonly int OutlineThresholdID = Shader.PropertyToID("_OutlineThreshold");
            private static readonly int DepthThresholdID = Shader.PropertyToID("_DepthThreshold");
            private static readonly int NormalThresholdID = Shader.PropertyToID("_NormalThreshold");
            private static readonly int PosterizeID = Shader.PropertyToID("_Posterize");
            private static readonly int PosterizationCountID = Shader.PropertyToID("_PosterizationCount");
            private static readonly int UseNormalsID = Shader.PropertyToID("_UseNormals");
            private static readonly int UseCannyID = Shader.PropertyToID("_UseCanny");

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
                    _material.SetFloat(DepthThresholdID, _settings.depthThreshold);
                    _material.SetFloat(NormalThresholdID, _settings.normalThreshold);
                    _material.SetFloat(PosterizeID, _settings.posterize ? 1 : 0);
                    _material.SetFloat(PosterizationCountID, _settings.posterizationCount);

                    // Set shader keywords
                    if (_settings.useNormals)
                        _material.EnableKeyword(USE_NORMALS_KEYWORD);
                    else
                        _material.DisableKeyword(USE_NORMALS_KEYWORD);

                    if (_settings.useCanny)
                        _material.EnableKeyword(USE_CANNY_KEYWORD);
                    else
                        _material.DisableKeyword(USE_CANNY_KEYWORD);

                    if (_settings.posterize)
                        _material.EnableKeyword(POSTERIZE_KEYWORD);
                    else
                        _material.DisableKeyword(POSTERIZE_KEYWORD);

                    // Add blit pass
                    RenderGraphUtils.BlitMaterialParameters para = new(source, destination, _material, 0);
                    renderGraph.AddBlitPass(para, "Outline Blit Pass");

                    // Copy back
                    RenderGraphUtils.BlitMaterialParameters paraCopy = new(destination, source, _material, 0);
                    renderGraph.AddBlitPass(paraCopy, "Outline Copy Pass");
                }
            }

            public override void OnCameraCleanup(CommandBuffer cmd) => _tempTexture?.Release();
        }
    }
}