Shader "Custom/MandelbrotBlit"
{
    Properties
    {
        [Header(Mandelbrot Settings)]
        _Zoom("Zoom", Float) = 1.0
        _CenterX("Center X", Float) = -0.5
        _CenterY("Center Y", Float) = 0.0
        _MaxIterations("Max Iterations", Range(1, 500)) = 100

        [Header(Coloring)]
        _ColorMultiplier("Color Multiplier", Float) = 1.0
        _ColorOffset("Color Offset", Float) = 0.0
        [Toggle(USE_SMOOTH_COLOR)] _SmoothColor("Smooth Coloring", Float) = 1

        [Header(Animation)]
        [Toggle(ANIMATE_ZOOM)] _AnimateZoom("Animate Zoom", Float) = 0
        _ZoomSpeed("Zoom Speed", Float) = 0.5
        [Toggle(ANIMATE_PAN)] _AnimatePan("Animate Pan", Float) = 0
        _PanSpeed("Pan Speed", Float) = 0.1

        [Header(Gradient)]
        _GradientTex("Color Gradient", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "MandelbrotPass"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #pragma shader_feature_local USE_SMOOTH_COLOR
            #pragma shader_feature_local ANIMATE_ZOOM
            #pragma shader_feature_local ANIMATE_PAN

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_GradientTex);
            SAMPLER(sampler_GradientTex);

            CBUFFER_START(UnityPerMaterial)
                float _Zoom;
                float _CenterX;
                float _CenterY;
                int _MaxIterations;
                float _ColorMultiplier;
                float _ColorOffset;
                float _ZoomSpeed;
                float _PanSpeed;
            CBUFFER_END

            // Mandelbrot iteration function
            float mandelbrot(float2 c, int maxIter)
            {
                float2 z = float2(0.0, 0.0);
                int iter = 0;

                for (iter = 0; iter < maxIter; iter++)
                {
                    // z = z^2 + c
                    float x = (z.x * z.x - z.y * z.y) + c.x;
                    float y = (2.0 * z.x * z.y) + c.y;

                    z = float2(x, y);

                    // Check if we've escaped
                    if (length(z) > 2.0)
                        break;
                }

                #ifdef USE_SMOOTH_COLOR
                    // Smooth coloring for better gradients
                    if (iter < maxIter)
                    {
                        float log_zn = log(length(z));
                        float nu = log(log_zn / log(2.0)) / log(2.0);
                        return float(iter) + 1.0 - nu;
                    }
                #endif

                return float(iter);
            }

            // HSV to RGB conversion for procedural colors
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Get UV coordinates (0 to 1)
                float2 uv = input.texcoord;

                // Convert to centered coordinates (-aspect to +aspect for x, -1 to 1 for y)
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 coord = (uv - 0.5) * 2.0;
                coord.x *= aspect;

                // Apply zoom and center
                float zoom = _Zoom;
                float2 center = float2(_CenterX, _CenterY);

                #ifdef ANIMATE_ZOOM
                    // Exponential zoom animation
                    zoom *= exp(-_Time.y * _ZoomSpeed);
                #endif

                #ifdef ANIMATE_PAN
                    // Circular pan animation
                    center.x += sin(_Time.y * _PanSpeed) * 0.3;
                    center.y += cos(_Time.y * _PanSpeed) * 0.3;
                #endif

                coord = coord / zoom + center;

                // Calculate Mandelbrot set
                float iterations = mandelbrot(coord, _MaxIterations);

                // Normalize iterations to 0-1 range
                float t = iterations / float(_MaxIterations);

                // Check if point is in the set (black interior)
                if (iterations >= float(_MaxIterations))
                {
                    return float4(0, 0, 0, 1);
                }

                // Apply color multiplier and offset for variety
                t = frac(t * _ColorMultiplier + _ColorOffset);

                // Sample gradient texture
                float3 color = SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(t, 0.5)).rgb;

                // Alternative: Procedural HSV coloring (uncomment to use)
                // float3 color = hsv2rgb(float3(t, 0.8, 0.9));

                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}