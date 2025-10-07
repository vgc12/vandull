Shader "Hidden/OutlineBlit"
{

    Properties
    {
        _OutlineThickness ("Outline Thickness", Float) = 1.0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)

    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "OutlinePass"
            Cull Off
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
                float _OutlineThreshold;
            CBUFFER_END

            // Sobel kernels for edge detection
            static const float sobelX[9] =
            {
                -1, 0, 1,
                -2, 0, 2,
                -1, 0, 1
            };

            static const float sobelY[9] =
            {
                -1, -2, -1,
                0, 0, 0,
                1, 2, 1
            };

            // Sample offsets for 3x3 kernel
            static const float2 sobelSamplePoints[9] =
            {
                float2(-1, 1), float2(0, 1), float2(1, 1),
                float2(-1, 0), float2(0, 0), float2(1, 0),
                float2(-1, -1), float2(0, -1), float2(1, -1)
            };

            // Sobel depth edge detection
            float GetDepthEdgeSobel(float2 uv, float2 texelSize)
            {
                float sobelGradientX = 0;
                float sobelGradientY = 0;

                [unroll]
                for (int i = 0; i < 9; i++)
                {
                    float2 sampleUV = uv + sobelSamplePoints[i] * texelSize;
                    float depth = SampleSceneDepth(sampleUV);

                    // Convert to linear depth for better edge detection
                    depth = Linear01Depth(depth, _ZBufferParams);

                    sobelGradientX += depth * sobelX[i];
                    sobelGradientY += depth * sobelY[i];
                }

                // Calculate gradient magnitude
                return sqrt(sobelGradientX * sobelGradientX + sobelGradientY * sobelGradientY);
            }

            // Sobel normal edge detection
            float GetNormalEdgeSobel(float2 uv, float2 texelSize)
            {
                float3 sobelGradientX = float3(0, 0, 0);
                float3 sobelGradientY = float3(0, 0, 0);

                [unroll]
                for (int i = 0; i < 9; i++)
                {
                    float2 sampleUV = uv + sobelSamplePoints[i] * texelSize;
                    float3 normal = SampleSceneNormals(sampleUV);

                    sobelGradientX += normal * sobelX[i];
                    sobelGradientY += normal * sobelY[i];
                }

                // Calculate gradient magnitude for each normal component
                // Use the maximum to capture edges in any direction
                float edgeX = length(sobelGradientX);
                float edgeY = length(sobelGradientY);

                return sqrt(edgeX * edgeX + edgeY * edgeY);
            }

            float SampleDepth(float2 uv)
            {
                #if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
                return SAMPLE_TEXTURE2D_ARRAY(_CameraDepthTexture, sampler_CameraDepthTexture, uv, unity_StereoEyeIndex).r;
                #else
                return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
                #endif
            }

            float sobel(float2 uv)
            {
                float2 delta = float2(_OutlineThickness, _OutlineThickness);

                float hr = 0;
                float vt = 0;

                hr += SampleDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
                hr += SampleDepth(uv + float2(1.0, -1.0) * delta) * -1.0;
                hr += SampleDepth(uv + float2(-1.0, 0.0) * delta) * 2.0;
                hr += SampleDepth(uv + float2(1.0, 0.0) * delta) * -2.0;
                hr += SampleDepth(uv + float2(-1.0, 1.0) * delta) * 1.0;
                hr += SampleDepth(uv + float2(1.0, 1.0) * delta) * -1.0;

                vt += SampleDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
                vt += SampleDepth(uv + float2(0.0, -1.0) * delta) * 2.0;
                vt += SampleDepth(uv + float2(1.0, -1.0) * delta) * 1.0;
                vt += SampleDepth(uv + float2(-1.0, 1.0) * delta) * -1.0;
                vt += SampleDepth(uv + float2(0.0, 1.0) * delta) * -2.0;
                vt += SampleDepth(uv + float2(1.0, 1.0) * delta) * -1.0;

                return sqrt(hr * hr + vt * vt);
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                //float edge = sobel(input.texcoord);
                float s = pow(1 - saturate(sobel(input.texcoord)), 50);
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);

                // Binary threshold: either full outline or full texture
                // If edge is above threshold, use outline color, otherwise use original texture
                // float isEdge = step(_OutlineThreshold, edge);

                //return lerp(col, _OutlineColor, isEdge);
                return col * s;
            }
            ENDHLSL
        }
    }
}