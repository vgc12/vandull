Shader "Custom/URP_XRay"
{
    Properties
    {

        _XRayColor("X-Ray Color (Occluded)", Color) = (0, 1, 1, 1)
        _VisibleColor("Visible Color", Color) = (0, 0.5, 1, 1)


        _XRayIntensity("X-Ray Intensity", Range(0, 5)) = 2.0
        _RimPower("Rim Power", Range(0.1, 10)) = 3.0
        _RimIntensity("Rim Intensity", Range(0, 5)) = 1.5

        [Header(Texture)]
        _MainTex("Texture", 2D) = "white" {}
        _TextureBlend("Texture Blend", Range(0, 1)) = 0.3

        [Header(Rendering)]
        _Alpha("Alpha", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        // Pass 1: Render occluded parts (fails depth test)
        Pass
        {
            Name "XRayOccluded"
            Tags {}

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Greater // Only render when BEHIND other objects
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _XRayColor;
                float _XRayIntensity;
                float _RimPower;
                float _RimIntensity;
                float _Alpha;
                float _TextureBlend;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Normalize vectors
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);

                // Rim lighting (Fresnel effect)
                float NdotV = saturate(dot(normalWS, viewDirWS));
                float rim = pow(1.0 - NdotV, _RimPower) * _RimIntensity;

                // Sample texture
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Combine rim with color
                float3 xrayColor = _XRayColor.rgb * _XRayIntensity;
                xrayColor += rim * _XRayColor.rgb;

                // Blend with texture
                xrayColor = lerp(xrayColor, xrayColor * texColor.rgb, _TextureBlend);

                return float4(xrayColor, _Alpha);
            }
            ENDHLSL
        }

        // Pass 2: Render visible parts (passes depth test)
        Pass
        {
            Name "XRayVisible"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual // Only render when IN FRONT of other objects
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _VisibleColor;
                float _RimPower;
                float _RimIntensity;
                float _Alpha;
                float _TextureBlend;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Normalize vectors
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);

                // Rim lighting
                float NdotV = saturate(dot(normalWS, viewDirWS));
                float rim = pow(1.0 - NdotV, _RimPower) * _RimIntensity;

                // Sample texture
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Visible color with rim
                float3 visibleColor = _VisibleColor.rgb;
                visibleColor += rim * _VisibleColor.rgb;

                // Blend with texture
                visibleColor = lerp(visibleColor, visibleColor * texColor.rgb, _TextureBlend);

                return float4(visibleColor, _Alpha);
            }
            ENDHLSL
        }
    }
}