#ifndef VANDULL_PBR_INCLUDED
#define VANDULL_PBR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "VandullVFXFunctions.hlsl"


half3 VandullLightingPBR(BRDFData brdfData, BRDFData brdfDataClearCoat,
                         half3 lightColor, half3 lightDirectionWS, float lightAttenuation,
                         half3 normalWS, half3 viewDirectionWS,
                         half clearCoatMask, bool specularHighlightsOff)
{
    half NdotL = saturate(dot(normalWS, lightDirectionWS));


    half3 radiance = lightColor * (lightAttenuation * NdotL);
    radiance = celBanding(radiance, GetCelBandsRadiance());


    half3 brdf = brdfData.diffuse;
    #ifndef _SPECULARHIGHLIGHTS_OFF
    [branch] if (!specularHighlightsOff)
    {
        brdf += brdfData.specular * DirectBRDFSpecular(brdfData, normalWS, lightDirectionWS, viewDirectionWS);

        #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    
        half brdfCoat = kDielectricSpec.r * DirectBRDFSpecular(brdfDataClearCoat, normalWS, lightDirectionWS, viewDirectionWS);

        // Mix clear coat and base layer using khronos glTF recommended formula
        // https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md
        // Use NoV for direct too instead of LoH as an optimization (NoV is light invariant).
        half NoV = saturate(dot(normalWS, viewDirectionWS));
      
        // Use slightly simpler fresnelTerm (Pow4 vs Pow5) as a small optimization.
        // It is matching fresnel used in the GI/Env, so should produce a consistent clear coat blend (env vs. direct)
        half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * Pow4(1.0 - NoV);

        brdf = brdf * (1.0 - clearCoatMask * coatFresnel) + brdfCoat * clearCoatMask;
        #endif // _CLEARCOAT
    }
    #endif // _SPECULARHIGHLIGHTS_OFF

    return brdf * radiance;
}


half3 VandullLightingPBR(BRDFData brdfData, BRDFData brdfDataClearCoat, Light light, half3 normalWS,
                         half3 viewDirectionWS, half clearCoatMask, bool specularHighlightsOff)
{
    return VandullLightingPBR(brdfData, brdfDataClearCoat, light.color, light.direction,
                              light.distanceAttenuation * light.shadowAttenuation, normalWS, viewDirectionWS,
                              clearCoatMask, specularHighlightsOff);
}

// Backwards compatibility
half3 VandullLightingPBR(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS)
{
    #ifdef _SPECULARHIGHLIGHTS_OFF
    bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    const BRDFData noClearCoat = (BRDFData)0;
    return VandullLightingPBR(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff);
}

half3 VandullLightingPBR(BRDFData brdfData, half3 lightColor, half3 lightDirectionWS, float lightAttenuation,
                         half3 normalWS, half3 viewDirectionWS)
{
    Light light;
    light.color = lightColor;
    light.direction = lightDirectionWS;
    light.distanceAttenuation = lightAttenuation;
    light.shadowAttenuation = 1;
    return VandullLightingPBR(brdfData, light, normalWS, viewDirectionWS);
}

half3 VandullLightingPBR(BRDFData brdfData, Light light, half3 normalWS, half3 viewDirectionWS,
                         bool specularHighlightsOff)
{
    const BRDFData noClearCoat = (BRDFData)0;
    return VandullLightingPBR(brdfData, noClearCoat, light, normalWS, viewDirectionWS, 0.0, specularHighlightsOff);
}

half3 VandullLightingPBR(BRDFData brdfData, half3 lightColor, half3 lightDirectionWS, float lightAttenuation,
                         half3 normalWS, half3 viewDirectionWS, bool specularHighlightsOff)
{
    Light light;
    light.color = lightColor;
    light.direction = lightDirectionWS;
    light.distanceAttenuation = lightAttenuation;
    light.shadowAttenuation = 1;
    return VandullLightingPBR(brdfData, light, viewDirectionWS, specularHighlightsOff, specularHighlightsOff);
}


half4 VandullPBR(InputData inputData, SurfaceData surfaceData)
{
    #if defined(_SPECULARHIGHLIGHTS_OFF)
                bool specularHighlightsOff = true;
    #else
    bool specularHighlightsOff = false;
    #endif
    BRDFData brdfData;

    // NOTE: can modify "surfaceData"...
    InitializeBRDFData(surfaceData, brdfData);

    #if defined(DEBUG_DISPLAY)
                half4 debugColor;

                if (CanDebugOverrideOutputColor(inputData, surfaceData, brdfData, debugColor))
                {
                    return debugColor;
                }
    #endif

    // Clear-coat calculation...
    BRDFData brdfDataClearCoat = CreateClearCoatBRDFData(surfaceData, brdfData);
    half4 shadowMask = CalculateShadowMask(inputData);
    AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfaceData);
    uint meshRenderingLayers = GetMeshRenderingLayer();
    Light mainLight = GetMainLight(inputData, shadowMask, aoFactor);

    // NOTE: We don't apply AO to the GI here because it's done in the lighting calculation below...
    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI);

    LightingData lightingData = CreateLightingData(inputData, surfaceData);

    lightingData.giColor = GlobalIllumination(brdfData, brdfDataClearCoat, surfaceData.clearCoatMask,
                                              inputData.bakedGI, aoFactor.indirectAmbientOcclusion,
                                              inputData.positionWS,
                                              inputData.normalWS, inputData.viewDirectionWS,
                                              inputData.normalizedScreenSpaceUV);
    #ifdef _LIGHT_LAYERS
    if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
    #endif
    {
        lightingData.mainLightColor = VandullLightingPBR(brdfData, brdfDataClearCoat,
                                                         mainLight,
                                                         inputData.normalWS, inputData.viewDirectionWS,
                                                         surfaceData.clearCoatMask,
                                                         specularHighlightsOff);
    }

    #if defined(_ADDITIONAL_LIGHTS)
    uint pixelLightCount = GetAdditionalLightsCount();

    #if USE_CLUSTER_LIGHT_LOOP
    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
    {
        CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK

        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

    #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
        {
            lightingData.additionalLightsColor += VandullLightingPBR(brdfData, brdfDataClearCoat, light,
                                                                          inputData.normalWS, inputData.viewDirectionWS,
                                                                          surfaceData.clearCoatMask, specularHighlightsOff);
        }
    }
    #endif

    LIGHT_LOOP_BEGIN(pixelLightCount)
        Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

    #ifdef _LIGHT_LAYERS
        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
    #endif
        {
            lightingData.additionalLightsColor += VandullLightingPBR(
                brdfData, brdfDataClearCoat, light,
                inputData.normalWS, inputData.viewDirectionWS,
                surfaceData.clearCoatMask, specularHighlightsOff);
        }
    LIGHT_LOOP_END
    #endif

    #if defined(_ADDITIONAL_LIGHTS_VERTEX)
    lightingData.vertexLightingColor += inputData.vertexLighting * brdfData.diffuse;
    #endif

    #if REAL_IS_HALF
    // Clamp any half.inf+ to HALF_MAX
    return min(CalculateFinalColor(lightingData, surfaceData.alpha), HALF_MAX);
    #else
    return CalculateFinalColor(lightingData, surfaceData.alpha);
    #endif
}

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
    float4 tangentWS : TEXCOORD2;
    float fogFactor : TEXCOORD3;
    float2 staticLightmapUV : TEXCOORD4;
    float2 dynamicLightmapUV : TEXCOORD5;
    half3 vertexSH : TEXCOORD6;
    float2 uv : TEXCOORD7;
};

TEXTURE2D(_AlbedoMap);
SAMPLER(sampler_AlbedoMap);
TEXTURE2D(_MetallicMap);
SAMPLER(sampler_MetallicMap);
TEXTURE2D(_SpecularMap);
SAMPLER(sampler_SpecularMap);
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
    float4 _SpecularColor;
    float4 _TextureTiling;
    float4 _TextureOffset;
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

Varyings VandullPBRVert(Attributes input)
{
    Varyings output = (Varyings)0;

    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS);
    VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    output.positionCS = positionInputs.positionCS;
    output.positionWS = positionInputs.positionWS;
    output.normalWS = normalInputs.normalWS;
    output.tangentWS = float4(normalInputs.tangentWS, input.tangentOS.w);
    output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
    output.uv = input.uv * _TextureTiling.xy + _TextureOffset.xy;

    // Lightmap UVs
    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
    OUTPUT_LIGHTMAP_UV(input.dynamicLightmapUV, unity_DynamicLightmapST, output.dynamicLightmapUV);

    // SH/Light probe data for dynamic objects
    OUTPUT_SH(output.normalWS, output.vertexSH);

    return output;
}


float4 VandullPBRFrag(Varyings input) : SV_Target
{
    // Sample textures
    float4 albedoSample = SAMPLE_TEXTURE2D(_AlbedoMap, sampler_AlbedoMap, input.uv);
    half3 albedo = albedoSample.rgb * _Albedo.rgb;

    half metallic = 0.0h;
    half3 specular = half3(0.0h, 0.0h, 0.0h);

    // Sample based on workflow mode
    #if defined(_WORKFLOWMODE_SPECULAR)
                    // Specular workflow
                    specular = SAMPLE_TEXTURE2D(_SpecularMap, sampler_SpecularMap, input.uv).rgb * _SpecularColor.rgb;
                    metallic = 0.0h;
    #else
    // Metallic workflow (default)
    metallic = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, input.uv).r * _Metallic;
    specular = half3(0.0h, 0.0h, 0.0h);
    #endif

    half roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, input.uv).r * _Roughness;
    half smoothness = 1.0h - roughness;
    half occlusion = SAMPLE_TEXTURE2D(_AOMap, sampler_AOMap, input.uv).r;
    occlusion = lerp(1.0h, occlusion, _AO);
    half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb
        * _EmissionColor.rgb * _EmissionStrength;

    // === SAMPLE AND TRANSFORM NORMAL MAP ===
    half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
    normalTS.xy *= _NormalStrength;
    normalTS = normalize(normalTS);

    if ((checkNormalThreshold(normalTS.x, _NormalThreshold) && _ColorX) ||
        (checkNormalThreshold(normalTS.y, _NormalThreshold) && _ColorY) ||
        (checkNormalThreshold(normalTS.z, _NormalThreshold) && _ColorZ))
    {
        return _NormalEffectsColor;
    }

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
    inputData.tangentToWorld = tangentToWorld;

    // Setup SurfaceData
    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo = albedo;
    surfaceData.metallic = metallic;
    surfaceData.specular = specular;
    surfaceData.smoothness = smoothness;
    surfaceData.occlusion = occlusion;
    surfaceData.emission = emission;
    surfaceData.normalTS = normalTS;
    surfaceData.alpha = 1.0;

    float4 color = VandullPBR(inputData, surfaceData);

    return color;
}


#endif
