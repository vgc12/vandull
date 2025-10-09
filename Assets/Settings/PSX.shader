Shader "Hidden/PSXBlit"
{
    Properties
    {
        _PixelSize("Pixel Size", Float) = 1
        [Toggle(Dither)] _Dither("Enable Dither", Float) = 0
        _DitherStrength("Dither Strength", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "PSXPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #pragma shader_feature_local DITHER

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _PixelSize;
                #ifdef DITHER
                float _DitherStrength;
                #endif

            CBUFFER_END

            float bayer4x4(float2 pixelPos)
            {
                int x = int(pixelPos.x) % 4;
                int y = int(pixelPos.y) % 4;

                const float bayerMatrix[16] = {
                    0.0 / 16.0, 8.0 / 16.0, 2.0 / 16.0, 10.0 / 16.0,
                    12.0 / 16.0, 4.0 / 16.0, 14.0 / 16.0, 6.0 / 16.0,
                    3.0 / 16.0, 11.0 / 16.0, 1.0 / 16.0, 9.0 / 16.0,
                    15.0 / 16.0, 7.0 / 16.0, 13.0 / 16.0, 5.0 / 16.0
                };

                return bayerMatrix[y * 4 + x] - 0.5; // Center around 0
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Sample Color from BlitTexture
                float2 fragCoord = input.positionCS.xy;


                float x = float(int(fragCoord.x) % _PixelSize);
                float y = float(int(fragCoord.y) % _PixelSize);


                float2 offset = float2(
                    fragCoord.x + floor(_PixelSize / 2.0) - x,
                    fragCoord.y + floor(_PixelSize / 2.0) - y
                );
                float2 sampleUV = offset / _ScreenParams.xy;

                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, sampleUV);
                #ifdef DITHER
                float dither = bayer4x4(input.positionCS.xy);
                col += dither * _DitherStrength;
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}