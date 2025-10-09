Shader "Hidden/PosterizeBlit"
{
    Properties
    {
        _PosterizationCount("Posterization Count", Integer) = 7
        _PixelSize("PixelSize", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "PosterizePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                int _PosterizationCount;
            CBUFFER_END


            float4 frag(Varyings input) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, input.texcoord);
                col = pow(abs(col), 0.4545);
                float3 c = RgbToHsv(col);
                c.z = round(c.z * _PosterizationCount) / _PosterizationCount;
                col = float4(HsvToRgb(c), col.a);
                col = pow(abs(col), 2.2);
                return col;
            }
            ENDHLSL
        }

    }
}