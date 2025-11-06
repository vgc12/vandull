Shader "Custom/Vandull"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white"{}
        _BaseColor("Base Color", Color) = (1,1,1,1)

        [Toggle] _UseNormalMapInLightCalculations("Use Normal Map In Light Calculations", Float) = 0
        _BumpMap ("Normal Map", 2D) = "bump"{}
        _BumpScale ("Normal Strength", Range(0, 2)) = 1.0

        [Header(Surface)]
        _RoughnessMap("Roughness Map", 2D) = "white" {}
        _Roughness("Roughness", Range(0, 1)) = 0.5

        [Header(Alpha)]
        _AlphaMap("Alpha Map", 2D) = "white" {}
        _AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.5
        [Toggle] _UseAlphaMap("Use Alpha Map", Float) = 0

        [Header(Rim Lighting)]
        _RimStrength("Rim Strength", Range(0, 1)) = 0.5
        _RimAmount("Rim Amount", Range(0, 1)) = 0.7
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1

        [Header(Bands)]
        [IntRange] _DiffuseBands("Diffuse Bands", Range(1, 30)) =5
        [IntRange] _ShadowAttenuationBands("Shadow Attenuation Bands", Range(1, 30) ) =5
        [IntRange] _RimBands("Rim Bands", Range(1,30)) = 5

        [Header(Edge Softness)]
        _EdgeDiffuse("Edge Diffuse", Range(0, 1)) = 0.05
        _EdgeSpecular("Edge Specular", Range(0, 1)) = 0.1
        _EdgeDistanceAttenuation("Edge Distance Attenuation", Range(0, 1)) = 0.05
        _EdgeShadowAttenuation("Edge Shadow Attenuation", Range(0, 1)) = 0.05
        _EdgeRim("Edge Rim", Range(0, 1)) = 0.05

        [Header(Ambient)]
        _AmbientColor("Ambient Color", Color) = (1,1,1,1)
        _AmbientMultiplier("Ambient Multiplier", Range(1, 10)) = 1

        [Header(Outline)]
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Range(0, 1)) = 0.02
        [Toggle(OUTLINE_METHOD_NORMAL)] _OutlineMethod("Extrude Outlines From Normals", Float) = 0

        [Header(Normal Effects)]
        _NormalThreshold("Normal Threshold", Range(0,1)) = .9999
        _NormalEffectsColor("Normal Effects Color", Color) = (0,0,0,1)
        [Toggle] _ColorX("Color X Direction", Float) = 1
        [Toggle] _ColorY("Color Y Direction", Float) = 1
        [Toggle] _ColorZ("Color Z Direction", Float) = 1

        [Header(Dither)]
        [Toggle(DITHER)] _Dither("Dither Enabled", Range(0,1)) = 0
        _DitherStrength("Dither Strength", Range(0, 1)) = 0.3

        [Header(Glitch)]
        [Toggle(LSDEFFECT)]_LSDEffect("LSD Effect", Range(0,1)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }

        // PASS 1: Outline Pass (rendered first, behind the object)
        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode"="SRPDefaultUnlit"
            }

            // Render only back faces
            Cull Front
            ZWrite On

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
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_AlphaMap);
            SAMPLER(sampler_AlphaMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float _OutlineMethod;
                float4 _AlphaMap_ST;
                float _AlphaCutoff;
                bool _UseAlphaMap;
            CBUFFER_END

            Varyings OutlineVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                #ifdef OUTLINE_METHOD_NORMAL
                    // Expand along normals
                    float3 normalOS = normalize(input.normalOS);
                    input.positionOS.xyz += normalOS * _OutlineWidth;
                #else
                    float3 normalOS = normalize(input.normalOS);
                    input.positionOS.xyz += normalOS * _OutlineWidth;
                    input.positionOS.xyz *= (1.0 + _OutlineWidth);
                #endif

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _AlphaMap);

                return output;
            }

            half4 OutlineFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Apply alpha clipping to outline as well
                if (_UseAlphaMap)
                {
                    float alpha = SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, input.uv).r;
                    clip(alpha - _AlphaCutoff);
                }

                half4 color = _OutlineColor;
                return color;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Cull Off
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma shader_feature_local POSTERIZE
            #pragma shader_feature_local DITHER
            #pragma shader_feature_local LSDEFFECT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_RoughnessMap);
            SAMPLER(sampler_RoughnessMap);
            TEXTURE2D(_AlphaMap);
            SAMPLER(sampler_AlphaMap);

            CBUFFER_START(UnityPerMaterial)
                bool _UseNormalMapInLightCalculations;

                //Bands
                int _DiffuseBands;
                int _ShadowAttenuationBands;
                int _RimBands;

                float4 _BaseMap_ST;
                float4 _BumpMap_ST;
                float _BumpScale;
                float4 _BaseColor;

                // Surface
                float _Roughness;
                float4 _RoughnessMap_ST;

                // Alpha
                float4 _AlphaMap_ST;
                float _AlphaCutoff;
                bool _UseAlphaMap;

                // Rim Lighting
                float _RimStrength;
                float _RimAmount;
                float _RimThreshold;

                // Edge Softness
                float _EdgeDiffuse;
                float _SpecularMap_ST;
                float _EdgeSpecular;
                float _EdgeDistanceAttenuation;
                float _EdgeShadowAttenuation;
                float _EdgeRim;

                //Ambient
                float4 _AmbientColor;
                float _AmbientMultiplier;

                //Normal Effects
                float _NormalThreshold;
                float4 _NormalEffectsColor;
                bool _ColorX;
                bool _ColorY;
                bool _ColorZ;

                //Posterization
                int _PosterizationCount;

                //Dither
                float _DitherStrength;

                //Glitch Effect
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
            };

            struct EdgeConstants
            {
                float diffuse;
                float specular;
                float rim;
                float distanceAttenuation;
                float shadowAttenuation;
            };

            struct SurfaceVariables
            {
                float roughness;
                float shininess;

                float rimStrength;
                float rimAmount;
                float rimThreshold;

                float3 normal;
                float3 view;

                EdgeConstants ec;
            };

            float celBanding(float value, float bands)
            {
                return floor(value * bands) / bands;
            }

            float3 CalculateCelShading(Light l, SurfaceVariables s)
            {
                float attenuation =
                    smoothstep(0.0f, s.ec.distanceAttenuation, l.distanceAttenuation) *
                    smoothstep(0.0f, s.ec.shadowAttenuation, l.shadowAttenuation);

                attenuation = celBanding(attenuation, _ShadowAttenuationBands);

                float diffuse = saturate(dot(s.normal, l.direction));
                diffuse *= attenuation;

                float3 h = SafeNormalize(l.direction + s.view);
                float specular = saturate(dot(s.normal, h));
                specular = pow(specular, s.shininess);
                specular *= diffuse;

                float rim = 1 - dot(s.view, s.normal);
                rim *= pow(abs(diffuse), s.rimThreshold);
                diffuse = celBanding(diffuse, _DiffuseBands);
                diffuse = smoothstep(0.0f, s.ec.diffuse, diffuse);
                specular = s.roughness * smoothstep(0.005f,
                                                    0.005f + s.ec.specular * s.roughness, specular);
                rim = celBanding(rim, _RimBands);
                rim = s.rimStrength * smoothstep(
                    s.rimAmount - 0.5f * s.ec.rim,
                    s.rimAmount + 0.5f * s.ec.rim,
                    rim
                );

                return l.color * (diffuse + max(specular, rim));
            }

            float3 LightingCelShaded(float Roughness,
                                     float RimStrength, float RimAmount, float RimThreshold,
                                     float3 Position, float3 Normal, float3 View, float EdgeDiffuse,
                                     float EdgeSpecular, float EdgeDistanceAttenuation,
                                     float EdgeShadowAttenuation, float EdgeRim, out float3 Color)
            {
                Color = half3(0.5f, 0.5f, 0.5f);

                SurfaceVariables s;
                s.roughness = Roughness;
                s.shininess = exp2(10 * Roughness + 1);
                s.rimStrength = RimStrength;
                s.rimAmount = RimAmount;
                s.rimThreshold = RimThreshold;
                s.normal = normalize(Normal);
                s.view = SafeNormalize(View);
                s.ec.diffuse = EdgeDiffuse;
                s.ec.specular = EdgeSpecular;
                s.ec.distanceAttenuation = EdgeDistanceAttenuation;
                s.ec.shadowAttenuation = EdgeShadowAttenuation;
                s.ec.rim = EdgeRim;

                #if SHADOWS_SCREEN
                       float4 clipPos = TransformWorldToHClip(Position);
                       float4 shadowCoord = ComputeScreenPos(clipPos);
                #else
                float4 shadowCoord = TransformWorldToShadowCoord(Position);
                #endif

                Light light = GetMainLight(shadowCoord);
                Color = CalculateCelShading(light, s);
                
                #ifdef _ADDITIONAL_LIGHTS
                int pixelLightCount = GetAdditionalLightsCount();
                for (int i = 0; i < pixelLightCount; i++)
                {
                    light = GetAdditionalLight(i, Position);
                    Color += CalculateCelShading(light, s);
                }
                #endif

                return Color;
            }

            float2 pixelateScreenSpace(float4 positionCS, float pixelSize)
            {
                // Get screen position in pixels
                float2 screenPos = positionCS.xy;

                // Quantize to pixel grid
                float2 pixelatedPos = floor(screenPos / pixelSize) * pixelSize;

                return pixelatedPos;
            }

            static const float3 vn1 = float3(0.0, 0.0, 1.0);
            static const float3 vn2 = float3(0.0, 1.0, 0.0);
            static const float3 vn3 = float3(0.0, 1.0, 1.0);
            static const float3 vn4 = float3(1.0, 0.0, 0.0);
            static const float3 vn5 = float3(1.0, 0.0, 1.0);
            static const float3 vn6 = float3(1.0, 1.0, 0.0);
            static const float3 vn7 = float3(1.0, 1.0, 1.0);

            float3 ghash(float3 p)
            {
                float3 o;
                // these constants are the matrix m
                // individual components multiplied, because the whole matrix multiplication produces float rounding differences from C# equivalent code (Bell pepper)
                o.x = 127.1 * p.x + 311.7 * p.y + 74.7 * p.z;
                o.y = 269.5 * p.x + 183.3 * p.y + 246.1 * p.z;
                o.z = 113.5 * p.x + 271.9 * p.y + 124.6 * p.z;
                float3 q = ((o * 0.025) + 8.0) * o;
                // the constants 4.25 and 8.0 found empirically to give similar noise distribution to the sin approach
                return -1.0 + 2.0 * frac(fmod(q, 289.0) * (1.0 / 41.0));
            }

            float gnoise(float3 p)
            {
                float3 i = floor(p);
                float3 f = p - i;

                float3 u = f * f * (3.0 - 2.0 * f);
                float4 a = float4(dot(ghash(i), f),
                                   dot(ghash(i + vn1), f - vn1),
                                   dot(ghash(i + vn2), f - vn2),
                                   dot(ghash(i + vn3), f - vn3));
                float4 b = float4(dot(ghash(i + vn4), f - vn4),
                                dot(ghash(i + vn5), f - vn5),
                                dot(ghash(i + vn6), f - vn6),
                                dot(ghash(i + vn7), f - vn7));

                float4 c = lerp(a, b, u.x);
                float2 rg = lerp(c.xy, c.zw, u.y);
                // Added 1.2 here because our old noise was stronger
                return 1.2 * lerp(rg.x, rg.y, u.z);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = normalInputs.tangentWS;
                output.bitangentWS = normalInputs.bitangentWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            bool cn(float normal)
            {
                return abs(normal > _NormalThreshold);
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample alpha map and perform alpha clipping
                if (_UseAlphaMap)
                {
                    float alpha = SAMPLE_TEXTURE2D(_AlphaMap, sampler_AlphaMap, input.uv).r;
                    clip(alpha - _AlphaCutoff);
                }

                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).r * _Roughness;
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv) * _BumpScale);

                float3 normalWS = input.normalWS;
                if (_UseNormalMapInLightCalculations)
                {
                    normalWS = TransformTangentToWorld(
                        normalTS, half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
                }

                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 ambient = (SampleSH(normalWS) * _AmbientMultiplier) + _AmbientColor;

                float3 color;
                LightingCelShaded(roughness, _RimStrength, _RimAmount, _RimThreshold, input.positionWS,
                                      normalWS,
                                      viewDirWS, _EdgeDiffuse, _EdgeSpecular,
                                      _EdgeDistanceAttenuation,
                                      _EdgeShadowAttenuation, _EdgeRim, color);
                color += ambient;
                
                if ((_ColorX && cn(normalTS.x)) || (_ColorY && cn(normalTS.y)) || (_ColorZ && cn(normalTS.z)))
                {
                    return _NormalEffectsColor;
                }

                float4 finalColor = float4(color, 1) * texColor;
                
                #ifdef LSDEFFECT
                    finalColor *= abs(sin(_Time.y * 3.214)); 
                #endif

                return finalColor;
            }
            ENDHLSL
        }
    }
}