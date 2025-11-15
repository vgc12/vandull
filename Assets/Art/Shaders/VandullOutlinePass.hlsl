#ifndef VANDULL_OUTLINE_PASS_INCLUDED
#define VANDULL_OUTLINE_PASS_INCLUDED


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

#endif
