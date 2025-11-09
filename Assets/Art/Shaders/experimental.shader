Shader "Custom/Vandull"
{
    Properties
    {
        [Header(Main Textures)]
        _AlbedoMap("Albedo Map", 2D) = "white" {}
        _Albedo("Albedo Tint", Color) = (1,1,1,1)
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0,2)) = 1.0
        _MetallicMap("Metallic Map", 2D) = "white" {}
        _Metallic("Metallic", Range(0,1)) = 0.0
        _RoughnessMap("Roughness Map", 2D) = "white" {}
        _Roughness("Roughness", Range(0,1)) = 0.5
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


        // ====================================================================
        // OUTLINE PASS
        // ====================================================================
        Pass
        {
            Name "Outline"
            Cull Front
            ZTest [_ZTestMode]

            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fog
            #pragma shader_feature OUTLINE_METHOD_NORMAL
            #pragma shader_feature_local USE_OUTLINE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END


            Varyings OutlineVert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                #ifdef USE_OUTLINE
                    float3 normalOS = normalize(input.normalOS);
                #ifdef OUTLINE_METHOD_NORMAL
                        input.positionOS.xyz += normalOS * _OutlineWidth;
                #else
                        input.positionOS.xyz += normalOS * _OutlineWidth;
                        input.positionOS.xyz *= (1.0 + _OutlineWidth);
                #endif
                #endif

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 OutlineFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                #ifdef USE_OUTLINE
                    return _OutlineColor;
                #else
                discard;
                return half4(0, 0, 0, 0);
                #endif
            }
            ENDHLSL
        }

        // ====================================================================
        // FORWARD LIT PASS - Using URP's BRDF Functions
        // ====================================================================
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
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fog

            #pragma multi_compile_fog

            #include "VandullFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            /*struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };*/
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
                float2 dynamicLightmapUV : TEXCOORD2;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2; // xyz: tangent, w: sign
                float fogFactor : TEXCOORD3;
                float2 staticLightmapUV : TEXCOORD4;
                float2 dynamicLightmapUV : TEXCOORD5;
                half3 vertexSH : TEXCOORD6; // SH for dynamic objects
                float2 uv : TEXCOORD7;
            };


            TEXTURE2D(_AlbedoMap);
            SAMPLER(sampler_AlbedoMap);
            TEXTURE2D(_MetallicMap);
            SAMPLER(sampler_MetallicMap);
            TEXTURE2D(_RoughnessMap);
            SAMPLER(sampler_RoughnessMap);
            TEXTURE2D(_AOMap);
            SAMPLER(sampler_AOMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _AlbedoMap_ST;
                float4 _Albedo;
                float4 _EmissionColor;
                float4 _NormalEffectsColor;
                float _Metallic;
                float _Roughness;
                float _AO;
                float _NormalStrength;
                float _EmissionStrength;
                float _CellBands;
                float _NormalThreshold;
                float _ColorX;
                float _ColorY;
                float _ColorZ;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = float4(normalInputs.tangentWS, input.tangentOS.w);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                output.uv = TRANSFORM_TEX(input.uv, _AlbedoMap);

                // Lightmap UVs
                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                OUTPUT_LIGHTMAP_UV(input.dynamicLightmapUV, unity_DynamicLightmapST, output.dynamicLightmapUV);

                // SH/Light probe data for dynamic objects
                OUTPUT_SH(output.normalWS, output.vertexSH);

                return output;
            }


            float4 frag(Varyings input) : SV_Target
            {
                // Sample textures
                float4 albedoSample = SAMPLE_TEXTURE2D(_AlbedoMap, sampler_AlbedoMap, input.uv);
                half3 albedo = albedoSample.rgb * _Albedo.rgb;

                half metallic = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, input.uv).r * _Metallic;
                half roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).r * _Roughness;
                half smoothness = 1.0h - roughness;
                half occlusion = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, input.uv).r;
                occlusion = lerp(1.0h, occlusion, _AO);
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb
                    * _EmissionColor.rgb * _EmissionStrength;

                // === SAMPLE AND TRANSFORM NORMAL MAP ===
                // Sample normal map (tangent space)
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));

                normalTS.xy *= _NormalStrength;
                normalTS = normalize(normalTS);

                // Build tangent-to-world matrix
                half3 bitangent = cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent, input.normalWS);

                // Transform normal from tangent space to world space
                half3 normalWS = normalize(mul(normalTS, tangentToWorld));

                // Setup InputData
                InputData inputData;

                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;


                inputData.normalWS = normalWS;

                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
                inputData.vertexLighting = half3(0, 0, 0);

                #if defined(LIGHTMAP_ON)
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
                #else
                inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
                #endif

                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

                // ALSO SET tangentToWorld for InputData (some lighting functions use it)
                inputData.tangentToWorld = tangentToWorld;

                // Setup SurfaceData
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = metallic;
                surfaceData.smoothness = smoothness;
                surfaceData.occlusion = occlusion;
                surfaceData.emission = emission;
                surfaceData.normalTS = normalTS;
                surfaceData.alpha = 1.0; // or your alpha value
                surfaceData.specular = half3(0, 0, 0); // metallic workflow

                float4 color = VandullPBR(inputData, surfaceData);

                return color;
            }
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };


            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 postionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(postionWS, normalWS, _LightDirection));
                positionCS = ApplyShadowClamping(positionCS);
                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;

                output.positionCS = GetShadowPositionHClip(input);

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }

    }
}