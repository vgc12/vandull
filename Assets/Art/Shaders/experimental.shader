Shader "Custom/URP_PBR"
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
        _AlphaMap("Alpha Map", 2D) = "white" {}
        
        
        [Header(Cell Shading)]
        _CellBands("Cel Shading Bands", Range(1, 30)) = 4

        [Header(Outline)]
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
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("__src", Integer) = 5
       [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("__dst", Integer) = 10
     
  
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        
        
        Pass
        {
            Name "Outline"
  
            Cull Front
  
            HLSLPROGRAM
            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag
            #pragma multi_compile_fog
            #pragma shader_feature OUTLINE_METHOD_NORMAL

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

                float3 normalOS = normalize(input.normalOS);

                #ifdef OUTLINE_METHOD_NORMAL
                    input.positionOS.xyz += normalOS * _OutlineWidth;
                #else
                input.positionOS.xyz += normalOS * _OutlineWidth;
                input.positionOS.xyz *= (1.0 + _OutlineWidth);
                #endif

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 OutlineFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return _OutlineColor;
            }
            ENDHLSL
        }
        

        Pass{
      
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

 
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fog

            #include "VandullFunctions.hlsl"    
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float fogFactor : TEXCOORD5;
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
            TEXTURE2D(_AlphaMap);
            SAMPLER(sampler_AlphaMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _AlbedoMap_ST;
                float4 _RoughnessMap_ST;
                float4 _MetallicMap_ST;
                float4 _AOMap_ST;
                float4 _NormalMap_ST;
                float4 _EmissionMap_ST;
                float4 _AlphaMap_ST;
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

            #define PI 3.14159265359

            // Distribution GGX
            float DistributionGGX(float3 N, float3 H, float roughness)
            {
                float a = roughness * roughness;
                float a2 = a * a;
                float NdotH = max(dot(N, H), 0.0);
                float NdotH2 = NdotH * NdotH;

                float nom = a2;
                float denom = (NdotH2 * (a2 - 1.0) + 1.0);
                denom = PI * denom * denom;

                return nom / denom;
            }

            // Geometry Schlick GGX
            float GeometrySchlickGGX(float NdotV, float roughness)
            {
                float r = (roughness + 1.0);
                float k = (r * r) / 8.0;

                float nom = NdotV;
                float denom = NdotV * (1.0 - k) + k;

                return nom / denom;
            }

            // Geometry Smith
            float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
            {
                float NdotV = max(dot(N, V), 0.0);
                float NdotL = max(dot(N, L), 0.0);
                float ggx2 = GeometrySchlickGGX(NdotV, roughness);
                float ggx1 = GeometrySchlickGGX(NdotL, roughness);

                return ggx1 * ggx2;
            }

            // Fresnel Schlick
            float3 FresnelSchlick(float cosTheta, float3 F0)
            {
                return F0 + (1.0 - F0) * pow(saturate(1.0 - cosTheta), 5.0);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.uv = TRANSFORM_TEX(input.uv, _AlbedoMap);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }


            float4 frag(Varyings input) : SV_Target
            {
                // Sample textures
                float4 albedoSample = SAMPLE_TEXTURE2D(_AlbedoMap, sampler_AlbedoMap, input.uv);
                float3 albedo = albedoSample.rgb * _Albedo.rgb;
                float alpha = SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, input.uv).r ;

                float metallic = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, input.uv).r * _Metallic;
                float roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).r * _Roughness;
                float ao = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, input.uv).r * _AO;
       
                
                // Sample emission
                float3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb * _EmissionStrength;
                
                // Sample and apply normal map
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));

                if ((_ColorX > 0.5 && checkNormalThreshold(normalTS.x, _NormalThreshold)) ||
                    (_ColorY > 0.5 && checkNormalThreshold(normalTS.y, _NormalThreshold)) ||
                    (_ColorZ > 0.5 && checkNormalThreshold(normalTS.z, _NormalThreshold)))
                {
                    return float4(_NormalEffectsColor.rgb, alpha);
                }

                normalTS.xy *= _NormalStrength;
                // Check normal threshold effects

                float3x3 TBN = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                float3 N = normalize(mul(normalTS, TBN));

                float3 V = normalize(GetCameraPositionWS() - input.positionWS);

                float3 F0 = float3(0.04, 0.04, 0.04);
                F0 = lerp(F0, albedo, metallic);

                float3 Lo = float3(0.0, 0.0, 0.0);

                // Main Light
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));

                float3 L = normalize(mainLight.direction);
                float3 H = normalize(V + L);
                float3 attenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                float3 radiance = mainLight.color * attenuation;

                // Cook-Torrance BRDF
                float NDF = D_GGX(dot(N, H), roughness);
                float G = GeometrySmith(N, V, L, roughness);
                float3 F = FresnelSchlick(max(dot(H, V), 0.0), F0);

                float3 kS = F;
                float3 kD = float3(1.0, 1.0, 1.0) - kS;
                kD *= 1.0 - metallic;

                float3 numerator = NDF * G * F;
                float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
                float3 specular = numerator / denominator;

                float NdotL = max(dot(N, L), 0.0);
                NdotL = celBanding(NdotL, _CellBands);
                Lo += (kD * albedo / PI + specular) * radiance * NdotL;


                // Additional Lights
                #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light light = GetAdditionalLight(lightIndex, input.positionWS);
                    
                    float3 L = normalize(light.direction);
                    float3 H = normalize(V + L);
                    float3 radiance = light.color * light.distanceAttenuation * light.shadowAttenuation;

                    // Cook-Torrance BRDF
                    float NDF = DistributionGGX(N, H, roughness);
                    float G = GeometrySmith(N, V, L, roughness);
                    float3 F = FresnelSchlick(max(dot(H, V), 0.0), F0);

                    float3 kS = F;
                    float3 kD = float3(1.0, 1.0, 1.0) - kS;
                    kD *= 1.0 - metallic;

                    float3 numerator = NDF * G * F;
                    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
                    float3 specular = numerator / denominator;
                    specular = celBanding(specular, _CellBands);

                    float NdotL = max(dot(N, L), 0.0);
                    NdotL = celBanding(NdotL, _CellBands);
                    Lo += (kD * albedo / PI + specular) * radiance * NdotL;
                }
                #endif

                // Ambient
                float3 ambient = float3(0.03, 0.03, 0.03) * albedo * ao;
                float3 color = ambient + Lo;

                // Add emission (before tone mapping for HDR glow)
                color += emission;

                // Tone mapping
                color = color / (color + float3(1.0, 1.0, 1.0));

                // Gamma correction
                color = pow(color, float3(1.0 / 2.2, 1.0 / 2.2, 1.0 / 2.2));

                // Apply fog
                color = MixFog(color, input.fogFactor);


                return float4(color, alpha);
            }
            ENDHLSL
        }
        // Depth only pass for depth pre-pass and depth-only rendering
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // Shadow caster pass for receiving shadows
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

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
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS =
                    TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));

                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

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