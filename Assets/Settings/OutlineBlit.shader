Shader "Custom/OutlineBlit"
{
    Properties
    {
        _OutlineThickness ("Outline Thickness", Float) = 1.0
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThreshold ("Outline Threshold", Range(0,1)) = 0.1
        _DepthThreshold ("Depth Threshold", Range(0,1)) = 0.1
        _NormalThreshold ("Normal Threshold", Range(0,1)) = 0.4
        _HighThreshold ("High Threshold", Range(0,1)) = 0.3
        _LowThreshold ("Low Threshold", Range(0,1)) = 0.1
        [Toggle(POSTERIZE)] _Posterize("Posterize", Float) = 1
        _PosterizationCount("Posterization Count", Float) = 99
        [Toggle(USE_CANNY)] _UseCanny("Use Canny Edge Detection", Float) = 1
        [Toggle(USE_NORMALS)] _UseNormals("Use Normals", Float) = 1
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
            #pragma shader_feature USE_NORMALS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineThickness;
                float4 _OutlineColor;
                float _OutlineThreshold;
                float _DepthThreshold;
                float _NormalThreshold;
                float _HighThreshold;
                float _LowThreshold;
                int _PosterizationCount;
            CBUFFER_END

            float SampleDepth(float2 uv)
            {
                return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
            }

            float3 SampleNormal(float2 uv)
            {
                return SampleSceneNormals(uv);
            }

            // Sobel operator for normals
            float sobelNormals(float2 uv)
            {
                float2 delta = float2(_OutlineThickness, _OutlineThickness);

                float3 n0 = SampleNormal(uv + float2(-1.0, -1.0) * delta);
                float3 n1 = SampleNormal(uv + float2(0.0, -1.0) * delta);
                float3 n2 = SampleNormal(uv + float2(1.0, -1.0) * delta);
                float3 n3 = SampleNormal(uv + float2(-1.0, 0.0) * delta);
                float3 n4 = SampleNormal(uv + float2(1.0, 0.0) * delta);
                float3 n5 = SampleNormal(uv + float2(-1.0, 1.0) * delta);
                float3 n6 = SampleNormal(uv + float2(0.0, 1.0) * delta);
                float3 n7 = SampleNormal(uv + float2(1.0, 1.0) * delta);

                // Sobel filter for normals
                float3 sobelX = n2 + 2.0 * n4 + n7 - n0 - 2.0 * n3 - n5;
                float3 sobelY = n0 + 2.0 * n1 + n2 - n5 - 2.0 * n6 - n7;

                float gradient = length(sobelX) + length(sobelY);
                return gradient;
            }

            // Sobel operator for depth
            float sobelDepth(float2 uv)
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

            float ApplyThreshold(float value, float threshold)
            {
                return step(threshold, value);
            }

            float4 frag(Varyings input) : SV_Target
            {
                float edgeFactor = 1.0;

                #ifdef USE_NORMALS
                // Use normals for edge detection
                float normalEdge = sobelNormals(input.texcoord);
                normalEdge = 1.0 - saturate(normalEdge * 2.0);
                edgeFactor *= pow(normalEdge, 20);
                edgeFactor = ApplyThreshold(edgeFactor, _NormalThreshold);
                
                // Optionally combine with depth for better results
                float depthEdge = sobelDepth(input.texcoord);
                depthEdge = pow(1.0 - saturate(depthEdge), 50);
                float depthFactor = ApplyThreshold(depthEdge, _DepthThreshold);
                
                // Multiply both (both must pass to avoid edge)
                edgeFactor = min(edgeFactor, depthFactor);
                #else
                #ifdef USE_CANNY
                    // Use Canny edge detection
                    edgeFactor = CannyEdgeDetection(input.texcoord);
                    edgeFactor = 1.0 - edgeFactor; // Invert for masking
                #else
                // Use original Sobel on depth
                float s = pow(1 - saturate(sobelDepth(input.texcoord)), 50);
                edgeFactor = ApplyThreshold(s, _OutlineThreshold);
                #endif
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