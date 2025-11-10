Shader "Custom/URP_XRay"
{
    Properties
    {

        [Header(Main Textures)]
        _AlbedoMap("Albedo Map", 2D) = "white" {}
        _Albedo("Albedo Tint", Color) = (1,1,1,1)
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0,2)) = 1.0

        [Header(Workflow)]
        [KeywordEnum(Metallic, Specular)] _WorkflowMode("Workflow Mode", Float) = 0

        [Header(Metallic Workflow)]
        _MetallicMap("Metallic Map", 2D) = "white" {}
        _Metallic("Metallic", Range(0,1)) = 0.0

        [Header(Specular Workflow)]
        _SpecularMap("Specular Map", 2D) = "white" {}
        _SpecularColor("Specular Color", Color) = (0.2, 0.2, 0.2, 1)

        [Header(Smoothness)]
        _RoughnessMap("Roughness Map", 2D) = "white" {}
        _Roughness("Roughness", Range(0,1)) = 0.5

        [Header(Other Maps)]
        _AOMap("AO Map", 2D) = "white" {}
        _AO("AO Strength", Range(0,1)) = 1.0
        _EmissionMap("Emission Map", 2D) = "black" {}
        _EmissionColor("Emission Color", Color) = (0,0,0,1)
        _EmissionStrength("Emission Strength", Range(0,10)) = 1.0

        [Header(Cell Shading)]
        _VandullCelBandsRadiance("Cel Shading Bands", Range(1, 30)) = 6.0

        [Header(Outline)]
        [Toggle(USE_OUTLINE)] _UseOutline("Enable Outline", Float) = 1
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Range(0, 1)) = 0.02
        [Toggle(OUTLINE_METHOD_NORMAL)] _OutlineMethod("Extrude Outlines From Normals", Float) = 0

        [Header(Normal Effects)]
        _NormalEffectsColor("Normal Effects Color", Color) = (1,0,0,1)
        _NormalThreshold("Normal Threshold", Range(0,1)) = 0.5
        [Toggle] _ColorX("Apply to X", Float) = 1
        [Toggle] _ColorY("Apply to Y", Float) = 1
        [Toggle] _ColorZ("Apply to Z", Float) = 1

        [Header(XRAY Settings)]
        [Toggle(XRAY_ENABLED)]
        _XRayEnabled("Enable X-Ray Effect", Float) = 1
        _XRayColor("X-Ray Color (Occluded)", Color) = (0, 1, 1, 1)
        _XRayIntensity("X-Ray Intensity", Range(0, 5)) = 2.0
        _RimPower("Rim Power", Range(0.1, 10)) = 3.0
        _RimIntensity("Rim Intensity", Range(0, 5)) = 1.5



        [Header(Rendering)]
        _Alpha("Alpha", Range(0, 1)) = 0.8

        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTestMode("__ztest", Integer) = 4


    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "XRayOccluded"
            Tags {}

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest [_ZTestMode]
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local XRAY_ENABLED
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


            CBUFFER_START(UnityPerMaterial)
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


                // Combine rim with color
                float3 xrayColor = _XRayColor.rgb * _XRayIntensity;
                xrayColor += rim * _XRayColor.rgb;

                // Blend with texture
                xrayColor = lerp(xrayColor, xrayColor, _TextureBlend);

                return float4(xrayColor, _Alpha);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Cull Front
            ZTest [_ZTestMode]
            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #pragma multi_compile_fog
            #pragma shader_feature_local OUTLINE_METHOD_NORMAL
            #pragma shader_feature_local USE_OUTLINE
            #include "VandullOutlinePass.hlsl"
            ENDHLSL
        }
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
            #pragma vertex VandullPBRVert
            #pragma fragment VandullPBRFrag


            #pragma shader_feature_local _WORKFLOWMODE_METALLIC _WORKFLOWMODE_SPECULAR

            #include "VandullPBR.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "VandullShadowPass.hlsl"
            ENDHLSL
        }

    }

    CustomEditor "VandullShaderGUI"
    Fallback "Universal Render Pipeline/Lit"
}