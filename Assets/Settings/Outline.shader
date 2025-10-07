Shader "Hidden/OutlineBlit"
{

    Properties
    {
        _OutlineThickness ("Outline Thickness", Float) = 1.0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        [Toggle(POSTERIZE)] _Posterize("Posterize", Float) = 1
        _PosterizationCount("Posterization Count", Float) = 99
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

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineThickness;
                float4 _OutlineColor;
                float _OutlineThreshold;
                int _PosterizationCount;
            CBUFFER_END


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
                //Get Sobel Factor
                float s = pow(1 - saturate(sobel(input.texcoord)), 50);
                // Sample Color from BlitTexture


                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

                col = pow(col, 0.4545);
                float3 c = RgbToHsv(col);
                c.z = round(c.z * _PosterizationCount) / _PosterizationCount;
                col = float4(HsvToRgb(c), col.a);
                col = pow(col, 2.2);

                return col * float4(s.xxx, 1);
            }
            ENDHLSL
        }
    }
}