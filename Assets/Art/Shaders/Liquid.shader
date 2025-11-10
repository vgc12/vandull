Shader "Custom/LiquidShader"
{
    Properties
    {
        _Color ("Liquid Color", Color) = (0.2, 0.5, 1.0, 0.8)
        _TopColor ("Top Color", Color) = (0.3, 0.6, 1.0, 0.8)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
        _WobbleX ("Wobble X", Float) = 0
        _WobbleZ ("Wobble Z", Float) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.8
        _Metallic ("Metallic", Range(0, 1)) = 0.1
        _FoamWidth ("Foam Width", Range(0, 0.2)) = 0.05
        _RimPower ("Rim Power", Range(0, 10)) = 3
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
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
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _TopColor;
                float4 _FoamColor;
                float _FillAmount;
                float _WobbleX;
                float _WobbleZ;
                float _Smoothness;
                float _Metallic;
                float _FoamWidth;
                float _RimPower;
                float _RimIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Calculate wobble effect on fill line
                float wobble = sin(_WobbleX + input.positionWS.x * 2.0) * 0.02 +
                    sin(_WobbleZ + input.positionWS.z * 2.0) * 0.02;

                // Get the fill plane in world space
                float fillPlane = _FillAmount * 2.0 - 1.0 + wobble;

                // Clip pixels below the fill line
                float fillEdge = input.positionWS.y - fillPlane;
                clip(fillEdge);

                // Foam at the top
                float foam = smoothstep(_FoamWidth, 0.0, fillEdge);

                // Color gradient from bottom to top
                float heightGradient = saturate((input.positionWS.y + 1.0) / 2.0);
                half4 liquidColor = lerp(_Color, _TopColor, heightGradient);

                // Mix in foam
                half4 finalColor = lerp(liquidColor, _FoamColor, foam);

                // Fresnel/rim effect
                float3 viewDir = normalize(input.viewDirWS);
                float3 normal = normalize(input.normalWS);
                float rim = 1.0 - saturate(dot(viewDir, normal));
                rim = pow(rim, _RimPower) * _RimIntensity;

                // Simple lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(normal, mainLight.direction));
                half3 lighting = mainLight.color * NdotL * 0.5 + 0.5;

                // Apply lighting and rim
                finalColor.rgb = finalColor.rgb * lighting + rim;

                // Metallic/smoothness effect (simple specular)
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float NdotH = saturate(dot(normal, halfDir));
                float specular = pow(NdotH, _Smoothness * 100.0) * _Metallic;
                finalColor.rgb += specular * mainLight.color;

                return finalColor;
            }
            ENDHLSL
        }

        // Shadow Caster Pass
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
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // Depth Only Pass
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}