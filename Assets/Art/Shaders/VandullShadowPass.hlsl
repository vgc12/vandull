#ifndef VANDULL_SHADOW_PASS_INCLUDED
#define VANDULL_SHADOW_PASS_INCLUDED

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
#endif
