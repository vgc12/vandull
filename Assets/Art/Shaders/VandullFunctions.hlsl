#ifndef VANDULL_FUNCTIONS_INCLUDED
#define VANDULL_FUNCTIONS_INCLUDED

// ============================================================================
// STRUCTURES
// ============================================================================

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


#ifndef VANDULL_CEL_BANDS_RADIANCE
#define VANDULL_CEL_BANDS_RADIANCE 6.0
#endif
float _VandullCelBandsRadiance;

float GetCelBandsRadiance()
{
    return _VandullCelBandsRadiance > 0 ? _VandullCelBandsRadiance : VANDULL_CEL_BANDS_RADIANCE;
}

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

half3 celBanding(half3 value, half bands)
{
    return floor(value * bands) / bands;
}

float celBanding(float value, float bands)
{
    return floor(value * bands) / bands;
}

float color_mask(float3 mask, float3 color, float mask_threshold, float mask_fuzziness)
{
    float d = distance(mask, color);
    return saturate(1.0 - smoothstep(mask_threshold, mask_threshold + mask_fuzziness, d));
}

bool checkNormalThreshold(float normal, float threshold)
{
    return abs(normal > threshold);
}


float3 ApplyHolographicEffect(
    float2 uv,
    float3 baseColor,
    float3 texColor,
    sampler2D foilMask,
    sampler2D holoGradient,
    sampler2D holoNoise,
    float3 foilColor,
    float holoThreshold,
    float holoFuzziness,
    float holoPeriod,
    float holoScroll,
    float holoDirection,
    float holoStrength,
    float time)
{
    float4 mask = tex2D(foilMask, uv);
    float4 noiseTex = tex2D(holoNoise, uv);

    // Calculate color similarity
    float textureSimilarity = color_mask(foilColor, texColor, holoThreshold, holoFuzziness);

    // Calculate gradient sample with time-based animation
    float2 gradientSample = float2(
        (uv.y * holoDirection + uv.x * (1.0 - holoDirection)) / 2.0,
        0.0
    );

    gradientSample += float2(time * holoScroll * 0.1, 0.0);
    gradientSample = frac(gradientSample + uv * holoPeriod);

    float4 gradientTex = tex2D(holoGradient, gradientSample);

    // Calculate effect strength
    float strength = holoStrength * mask.r * textureSimilarity;

    // Mix holographic color with base color
    float3 holoColor = gradientTex.rgb * (noiseTex.rgb * 2.0);
    return lerp(baseColor, holoColor, strength);
}

// ============================================================================
// MANDELBROT FUNCTIONS
// ============================================================================

float CalculateMandelbrot(float2 c, int maxIter)
{
    float2 z = float2(0.0, 0.0);
    int iter = 0;

    for (iter = 0; iter < maxIter; iter++)
    {
        // z = z^2 + c
        float x = (z.x * z.x - z.y * z.y) + c.x;
        float y = (2.0 * z.x * z.y) + c.y;
        z = float2(x, y);

        // Check if escaped
        if (length(z) > 2.0)
            break;
    }

    // Smooth coloring for better gradients
    if (iter < maxIter)
    {
        float log_zn = log(length(z));
        float nu = log(log_zn / log(2.0)) / log(2.0);
        return float(iter) + 1.0 - nu;
    }

    return float(iter);
}

// ============================================================================
// PBR Lighting Alternatives
// ============================================================================
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/BRDF.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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

#endif // VANDULL_FUNCTIONS_INCLUDED
