Shader "Custom/Vandull"
{
    Properties
    {
        [Header(Main Textures)]
        [MainTexture] _AlbedoMap("Albedo Map", 2D) = "white" {}
        _Albedo("Albedo Tint", Color) = (1,1,1,1)
        [NoScaleOffset] _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0,2)) = 1.0

        [Header(Workflow)]
        [KeywordEnum(Metallic, Specular)] _WorkflowMode("Workflow Mode", Float) = 0

        [Header(Metallic Workflow)]
        [NoScaleOffset] _MetallicMap("Metallic Map", 2D) = "white" {}
        _Metallic("Metallic", Range(0,1)) = 0.0

        [Header(Specular Workflow)]
        [NoScaleOffset]_SpecularMap("Specular Map", 2D) = "white" {}
        _SpecularColor("Specular Color", Color) = (0.2, 0.2, 0.2, 1)

        [Header(Smoothness)]
        [NoScaleOffset]_RoughnessMap("Roughness Map", 2D) = "white" {}
        _Roughness("Roughness", Range(0,1)) = 0.5

        [Header(Other Maps)]
        [NoScaleOffset]_AOMap("AO Map", 2D) = "white" {}
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



        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTestMode("__ztest", Integer) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

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
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            ZTest [_ZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex VandullPBRVert
            #pragma fragment VandullPBRFrag


            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN

            #pragma multi_compile_fog

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