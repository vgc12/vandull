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

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

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

#endif // VANDULL_FUNCTIONS_INCLUDED
