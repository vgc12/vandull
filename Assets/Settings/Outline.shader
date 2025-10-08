Shader "Hidden/OutlineBlit"
{
    Properties
    {
        _OutlineThickness ("Outline Thickness", Float) = 1.0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThreshold ("Outline Threshold", Range(0,1)) = 0.1
        _HighThreshold ("High Threshold", Range(0,1)) = 0.3
        _LowThreshold ("Low Threshold", Range(0,1)) = 0.1
        [Toggle(POSTERIZE)] _Posterize("Posterize", Float) = 1
        _PosterizationCount("Posterization Count", Float) = 99
        [Toggle(USE_CANNY)] _UseCanny("Use Canny Edge Detection", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "OutlinePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #pragma shader_feature USE_CANNY
            #pragma shader_feature POSTERIZE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineThickness;
                float4 _OutlineColor;
                float _OutlineThreshold;
                float _HighThreshold;
                float _LowThreshold;
                int _PosterizationCount;
            CBUFFER_END

            float SampleDepth(float2 uv)
            {
                return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
            }

            // Gaussian blur for noise reduction
            float GaussianBlur(float2 uv)
            {
                float2 delta = float2(_OutlineThickness, _OutlineThickness);
                
                // 5x5 Gaussian kernel
                float kernel[25] = {
                    2, 4, 5, 4, 2,
                    4, 9, 12, 9, 4,
                    5, 12, 15, 12, 5,
                    4, 9, 12, 9, 4,
                    2, 4, 5, 4, 2
                };
                
                float sum = 0;
                float weightSum = 159.0; // Sum of kernel weights
                
                for (int y = -2; y <= 2; y++)
                {
                    for (int x = -2; x <= 2; x++)
                    {
                        int index = (y + 2) * 5 + (x + 2);
                        sum += SampleDepth(uv + float2(x, y) * delta) * kernel[index];
                    }
                }
                
                return sum / weightSum;
            }

            // Sobel operator for gradient calculation
            void CalculateGradient(float2 uv, out float magnitude, out float direction)
            {
                float2 delta = float2(_OutlineThickness, _OutlineThickness);

                // Sobel X kernel
                float gx = 0;
                gx += SampleDepth(uv + float2(-1.0, -1.0) * delta) * -1.0;
                gx += SampleDepth(uv + float2(1.0, -1.0) * delta) * 1.0;
                gx += SampleDepth(uv + float2(-1.0, 0.0) * delta) * -2.0;
                gx += SampleDepth(uv + float2(1.0, 0.0) * delta) * 2.0;
                gx += SampleDepth(uv + float2(-1.0, 1.0) * delta) * -1.0;
                gx += SampleDepth(uv + float2(1.0, 1.0) * delta) * 1.0;

                // Sobel Y kernel
                float gy = 0;
                gy += SampleDepth(uv + float2(-1.0, -1.0) * delta) * -1.0;
                gy += SampleDepth(uv + float2(0.0, -1.0) * delta) * -2.0;
                gy += SampleDepth(uv + float2(1.0, -1.0) * delta) * -1.0;
                gy += SampleDepth(uv + float2(-1.0, 1.0) * delta) * 1.0;
                gy += SampleDepth(uv + float2(0.0, 1.0) * delta) * 2.0;
                gy += SampleDepth(uv + float2(1.0, 1.0) * delta) * 1.0;

                magnitude = sqrt(gx * gx + gy * gy);
                direction = atan2(gy, gx);
            }

            // Non-maximum suppression
            float NonMaximumSuppression(float2 uv, float magnitude, float direction)
            {
                float2 delta = float2(_OutlineThickness, _OutlineThickness);
                
                // Quantize direction to 0, 45, 90, 135 degrees
                float angle = degrees(direction);
                angle = angle < 0 ? angle + 180 : angle;
                
                float2 offset1, offset2;
                
                // Determine neighbor pixels based on gradient direction
                if ((angle >= 0 && angle < 22.5) || (angle >= 157.5 && angle <= 180))
                {
                    // Horizontal edge (0 degrees)
                    offset1 = float2(1, 0) * delta;
                    offset2 = float2(-1, 0) * delta;
                }
                else if (angle >= 22.5 && angle < 67.5)
                {
                    // Diagonal edge (45 degrees)
                    offset1 = float2(1, 1) * delta;
                    offset2 = float2(-1, -1) * delta;
                }
                else if (angle >= 67.5 && angle < 112.5)
                {
                    // Vertical edge (90 degrees)
                    offset1 = float2(0, 1) * delta;
                    offset2 = float2(0, -1) * delta;
                }
                else
                {
                    // Diagonal edge (135 degrees)
                    offset1 = float2(-1, 1) * delta;
                    offset2 = float2(1, -1) * delta;
                }
                
                // Get magnitudes of neighbors
                float mag1, mag2, dir1, dir2;
                CalculateGradient(uv + offset1, mag1, dir1);
                CalculateGradient(uv + offset2, mag2, dir2);
                
                // Suppress if not a local maximum
                if (magnitude >= mag1 && magnitude >= mag2)
                    return magnitude;
                else
                    return 0;
            }

            // Double threshold and hysteresis (simplified)
            float CannyEdgeDetection(float2 uv)
            {
                // 1. Gaussian blur (optional, can skip for performance)
                // float blurred = GaussianBlur(uv);
                
                // 2. Calculate gradient
                float magnitude, direction;
                CalculateGradient(uv, magnitude, direction);
                
                // 3. Non-maximum suppression
                float nms = NonMaximumSuppression(uv, magnitude, direction);
                
                // 4. Double threshold
                if (nms >= _HighThreshold)
                    return 1.0; // Strong edge
                else if (nms >= _LowThreshold)
                    return 0.5; // Weak edge (could be enhanced with hysteresis)
                else
                    return 0.0; // Not an edge
            }

            float sobel(float2 uv)
            {
                float2 delta = float2(_OutlineThickness, _OutlineThickness);

                float hr = 0;
                float vt = 0;

                hr += SampleDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
                hr += SampleDepth(uv + float2(1.0, -1.0) * delta) * -1.0;
                hr += SampleDepth(uv + float2(-1.0, 0.0) * delta) * 2.0;
                hr += SampleDepth(uv + float2(1.0, 0.0) * delta) * -2.0;
                hr += SampleDepth(uv + float2(-1.0, 1.0) * delta) * 1.0;
                hr += SampleDepth(uv + float2(1.0, 1.0) * delta) * -1.0;

                vt += SampleDepth(uv + float2(-1.0, -1.0) * delta) * 1.0;
                vt += SampleDepth(uv + float2(0.0, -1.0) * delta) * 2.0;
                vt += SampleDepth(uv + float2(1.0, -1.0) * delta) * 1.0;
                vt += SampleDepth(uv + float2(-1.0, 1.0) * delta) * -1.0;
                vt += SampleDepth(uv + float2(0.0, 1.0) * delta) * -2.0;
                vt += SampleDepth(uv + float2(1.0, 1.0) * delta) * -1.0;

                return sqrt(hr * hr + vt * vt);
            }

            float ApplyThreshold(float value)
            {
                return step(_OutlineThreshold, value);
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                float edgeFactor;
                
#ifdef USE_CANNY
                // Use Canny edge detection
                edgeFactor = CannyEdgeDetection(input.texcoord);
                edgeFactor = 1.0 - edgeFactor; // Invert for masking
#else
                // Use original Sobel
                float s = pow(1 - saturate(sobel(input.texcoord)), 50);
                edgeFactor = ApplyThreshold(s);
#endif
                
                // Sample Color from BlitTexture
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                
#ifdef POSTERIZE
                col = pow(col, 0.4545);
                float3 c = RgbToHsv(col);
                c.z = round(c.z * _PosterizationCount) / _PosterizationCount;
                col = float4(HsvToRgb(c), col.a);
                col = pow(col, 2.2);
#endif
                
                return col * edgeFactor;
            }
            ENDHLSL
        }
    }
}