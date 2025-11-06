Shader "Custom/VandullWithMandelbrot"
{
    Properties
    {
        // Base Properties
        _BaseMap("Base Map", 2D) = "white"{}
        _BaseColor("Base Color", Color) = (1,1,1,1)

        // Normal Map
        [Toggle] _UseNormalMapInLightCalculations("Use Normal Map In Light Calculations", Float) = 0
        _BumpMap ("Normal Map", 2D) = "bump"{}
        _BumpScale ("Normal Strength", Range(0, 2)) = 1.0

        [Header(Surface)]
        _RoughnessMap("Roughness Map", 2D) = "white" {}
        _Roughness("Roughness", Range(0, 1)) = 0.5

        [Header(Rim Lighting)]
        _RimStrength("Rim Strength", Range(0, 1)) = 0.5
        _RimAmount("Rim Amount", Range(0, 1)) = 0.7
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1

        [Header(Bands)]
        [IntRange] _DiffuseBands("Diffuse Bands", Range(1, 30)) = 5
        [IntRange] _ShadowAttenuationBands("Shadow Attenuation Bands", Range(1, 30)) = 5
        [IntRange] _RimBands("Rim Bands", Range(1, 30)) = 5

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
        _NormalThreshold("Normal Threshold", Range(0, 1)) = 0.9999
        _NormalEffectsColor("Normal Effects Color", Color) = (0,0,0,1)
        [Toggle] _ColorX("Color X Direction", Float) = 1
        [Toggle] _ColorY("Color Y Direction", Float) = 1
        [Toggle] _ColorZ("Color Z Direction", Float) = 1

        [Header(Mandelbrot)]
        [Toggle(ENABLE_MANDELBROT)] _EnableMandelbrot("Enable Mandelbrot", Float) = 1
        [KeywordEnum(UV, WorldSpace, ScreenSpace)] _MandelbrotMode("Mandelbrot Mode", Float) = 0
        [Toggle] _MandelbrotInfiniteZoom("Infinite Zoom", Float) = 1
        _MandelbrotZoomSpeed("Zoom Speed", Range(0.01, 2)) = 0.5
        _MandelbrotZoomCenterX("Zoom Center X", Range(-2, 2)) = -0.5
        _MandelbrotZoomCenterY("Zoom Center Y", Range(-2, 2)) = 0.0
        _MandelbrotScale("Manual Scale (when zoom off)", Range(0.1, 10)) = 3.0
        _MandelbrotOffsetX("Manual Offset X (when zoom off)", Range(-2, 2)) = -0.5
        _MandelbrotOffsetY("Manual Offset Y (when zoom off)", Range(-2, 2)) = 0.0
        [IntRange] _MandelbrotIterations("Mandelbrot Iterations", Range(10, 200)) = 100
        _MandelbrotColor1("Mandelbrot Color 1", Color) = (0,0,0.2,1)
        _MandelbrotColor2("Mandelbrot Color 2", Color) = (0,0.5,1,1)
        _MandelbrotColor3("Mandelbrot Color 3", Color) = (1,1,1,1)
        _MandelbrotStrength("Mandelbrot Strength", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        // ========================================================================
        // PASS 1: OUTLINE
        // ========================================================================
        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }
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

        // ========================================================================
        // PASS 2: FORWARD LIT WITH MANDELBROT
        // ========================================================================
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

            // Multi-compile variants
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            // Shader features
            #pragma shader_feature_local ENABLE_MANDELBROT
            #pragma multi_compile _MANDELBROTMODE_UV _MANDELBROTMODE_WORLDSPACE _MANDELBROTMODE_SCREENSPACE

            // Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "VandullFunctions.hlsl"

            // Texture declarations
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);
            TEXTURE2D(_RoughnessMap);
            SAMPLER(sampler_RoughnessMap);

            // Material properties
            CBUFFER_START(UnityPerMaterial)
                // Base properties
                float4 _BaseMap_ST;
                float4 _BaseColor;

                // Normal map
                float _UseNormalMapInLightCalculations;
                float4 _BumpMap_ST;
                float _BumpScale;

                // Surface
                float4 _RoughnessMap_ST;
                float _Roughness;

                // Rim lighting
                float _RimStrength;
                float _RimAmount;
                float _RimThreshold;

                // Bands
                int _DiffuseBands;
                int _ShadowAttenuationBands;
                int _RimBands;

                // Edge softness
                float _EdgeDiffuse;
                float _EdgeSpecular;
                float _EdgeDistanceAttenuation;
                float _EdgeShadowAttenuation;
                float _EdgeRim;

                // Ambient
                float4 _AmbientColor;
                float _AmbientMultiplier;

                // Normal effects
                float _NormalThreshold;
                float4 _NormalEffectsColor;
                float _ColorX;
                float _ColorY;
                float _ColorZ;

                // Mandelbrot properties
                float _MandelbrotInfiniteZoom;
                float _MandelbrotZoomSpeed;
                float _MandelbrotZoomCenterX;
                float _MandelbrotZoomCenterY;
                float _MandelbrotScale;
                float _MandelbrotOffsetX;
                float _MandelbrotOffsetY;
                int _MandelbrotIterations;
                float4 _MandelbrotColor1;
                float4 _MandelbrotColor2;
                float4 _MandelbrotColor3;
                float _MandelbrotStrength;
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
                float4 screenPos : TEXCOORD5;
            };

            // Vertex shader
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
                output.screenPos = ComputeScreenPos(output.positionCS);

                return output;
            }


            float3 kaliSetPattern(float2 st)
            {
                return float3(0, 0, 0);
                float2 uv = st * 2.5;
                //uv.x = _ScreenParams.x / _ScreenParams.y;
                float2 c = float2(-abs(sin(_Time.y * 0.4)) * .2, -abs(cos(_Time.y * 0.25)) * .2);

                float2 z = uv;
                float iterations = 0.0;
                int maxIterations = 228;
                const float escapeRadius = 8.0;

                for (int i = 0; i < maxIterations; i++)
                {
                    z = abs(z) / dot(z, z) + c;
                    if (dot(z, z) > Sq(escapeRadius))
                    {
                        break;
                    }
                    iterations++;
                }

                float3 color = float3(0, 0, 0);

                if (iterations >= maxIterations - 1)
                {
                    color = float3(0.0, 0.0, 0.0); // Black for the set itself
                }
                else
                {
                    float smoothIter = iterations + 1.0 - log(log(length(z))) / log(2.0);

                    // Use logarithmic mapping to compress the range
                    float hue = frac(smoothIter / 5.0); // Repeating bands
                    float3 paletteFrequency = float3(0.0, 0.33, 0.67);
                    color = 0.5 + 0.5 * cos(2.0 * PI * (hue + paletteFrequency));

                    // Subtle animated modulation (reduce intensity)
                    color *= 0.8 + 0.2 * cos(_Time.y * 0.5 + uv.xyx + float3(0, 2, 4));
                }
                return color;
            }

            // Fragment shader
            half4 frag(Varyings input) : SV_Target
            {
                // Sample base textures
                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).r * _Roughness;
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv), _BumpScale);

                // Calculate world space normal
                float3 normalWS = input.normalWS;
                if (_UseNormalMapInLightCalculations > 0.5)
                {
                    normalWS = TransformTangentToWorld(
                        normalTS, half3x3(input.tangentWS, input.bitangentWS, input.normalWS));
                    normalWS = normalize(normalWS);
                }

                // Calculate lighting
                float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 ambient = (SampleSH(normalWS) * _AmbientMultiplier) + _AmbientColor.rgb;

                float3 color;
                LightingCelShaded(
                    roughness,
                    _RimStrength,
                    _RimAmount,
                    _RimThreshold,
                    input.positionWS,
                    normalWS,
                    viewDirWS,
                    _EdgeDiffuse,
                    _EdgeSpecular,
                    _EdgeDistanceAttenuation,
                    _EdgeShadowAttenuation,
                    _EdgeRim,
                    _ShadowAttenuationBands,
                    _DiffuseBands,
                    _RimBands,
                    color
                );

                color += ambient;

                // Check normal threshold effects
                if ((_ColorX > 0.5 && checkNormalThreshold(normalTS.x, _NormalThreshold)) ||
                    (_ColorY > 0.5 && checkNormalThreshold(normalTS.y, _NormalThreshold)) ||
                    (_ColorZ > 0.5 && checkNormalThreshold(normalTS.z, _NormalThreshold)))
                {
                    return _NormalEffectsColor;
                }

                // Combine lighting with texture
                float4 finalColor = float4(color, 1) * texColor;

                float2 c = input.screenPos.xy / input.screenPos.w;
                c -= .5f;
                c.x *= _ScreenParams.x / _ScreenParams.y;
                float4 k = float4(kaliSetPattern(c), 1);

                return finalColor * k;
            }
            ENDHLSL
        }
    }
}