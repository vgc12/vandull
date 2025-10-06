Shader "Hidden/OutlineBlit"
{
    Properties
    {
        _OutlineThickness ("Outline Thickness", Float) = 1.0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _DepthThreshold ("Depth Threshold", Float) = 1.5
        _NormalThreshold ("Normal Threshold", Float) = 0.4
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "OutlinePass"
            
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                float _OutlineThickness;
                float4 _OutlineColor;
                float _DepthThreshold;
                float _NormalThreshold;
            CBUFFER_END
            
            // Roberts Cross depth edge detection
            float GetDepthEdge(float2 uv, float2 offset)
            {
                float depth1 = SampleSceneDepth(uv);
                float depth2 = SampleSceneDepth(uv + offset);
                float depth3 = SampleSceneDepth(uv + float2(offset.x, -offset.y));
                float depth4 = SampleSceneDepth(uv + float2(-offset.x, offset.y));
                
                // Convert to linear depth for better edge detection
                depth1 = Linear01Depth(depth1, _ZBufferParams);
                depth2 = Linear01Depth(depth2, _ZBufferParams);
                depth3 = Linear01Depth(depth3, _ZBufferParams);
                depth4 = Linear01Depth(depth4, _ZBufferParams);
                
                float edge1 = abs(depth1 - depth2);
                float edge2 = abs(depth3 - depth4);
                
                return sqrt(edge1 * edge1 + edge2 * edge2) * 100.0;
            }
            
            // Roberts Cross normal edge detection
            float GetNormalEdge(float2 uv, float2 offset)
            {
                float3 normal1 = SampleSceneNormals(uv);
                float3 normal2 = SampleSceneNormals(uv + offset);
                float3 normal3 = SampleSceneNormals(uv + float2(offset.x, -offset.y));
                float3 normal4 = SampleSceneNormals(uv + float2(-offset.x, offset.y));
                
                float3 edge1 = normal1 - normal2;
                float3 edge2 = normal3 - normal4;
                
                return sqrt(dot(edge1, edge1) + dot(edge2, edge2));
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 texelSize = _BlitTexture_TexelSize.xy * _OutlineThickness;
                
                // Get edges
                float depthEdge = GetDepthEdge(uv, texelSize);
                float normalEdge = GetNormalEdge(uv, texelSize);
                
                // Threshold edges
                float edge = depthEdge > _DepthThreshold ? 1.0 : 0.0;
                edge = max(edge, normalEdge > _NormalThreshold ? 1.0 : 0.0);
                
                // Sample original color
                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
                
                // Apply outline
                return lerp(color, _OutlineColor, edge);
            }
            ENDHLSL
        }
    }
}