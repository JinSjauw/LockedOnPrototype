Shader "Custom/FullscreenGrid"
{
    Properties
    {
        [Header(Grid Terrain)] [Space]
        [HDR]_GridColor("Grid Color", Color) = (1,1,1,1)
        [HDR]_GridLitColor("Grid Lit Color", Color) = (1,1,1,1)
        [HDR]_BackgroundColor("Background Color", Color) = (1,1,1,0)

        _CellSize("Cell Size", Float) = 1.0
        _ArmWidth("Arm Width", Float) = 0.05
        _ArmLength("Arm Length", Float) = 0.08

        _HeightCutoff("Height Cutoff", Float) = 5
        _HeightCutoffFeather("Height Cutoff Feather", Float) = 1

        _ScanEffectRadius("Scan Effect Radius", Float) = 0
        _ScanEffectRingWidth("Scan Ring Width", Float) = 2

        _EdgeGridRadius("Grid Mask Ring Radius", Float) = 1
        _EdgeGridWidth("Grid Mask Ring Width", Float) = 1
        _EdgeGridFeather("Grid Mask Ring Feather", Float) = 1
        _EdgeGridOuterFeather("Grid Mask Ring Outer Feather", Float) = 1
        _EdgeGridInnerFeather("Grid Mask Ring Inner Feather", Float) = 1

        _GridMaskRadius("Grid Mask Radius", Float) = 5.0
        _GridMaskFeather("Grid Mask Feather", Float) = 1.0
        _GridAlpha("Grid Alpha", Float) = 1.0

        _GridMaskInnerRadius("Grid Mask Inner Radius", Float) = 5.0
        _GridMaskInnerFeather("Grid Mask Inner Feather", Float) = 1.0

        _BackgroundMaskInnerRadius("Background Mask Inner Radius", Float) = 5.0
        _BackgroundMaskInnerFeather("BackGroundMask Mask Inner Feather", Float) = 1.0

        _BackgroundMaskRadius("Background Mask Radius", Float) = 5.0
        _BackgroundMaskFeather("Background Mask Feather", Float) = 1.0
        _BackgroundAlpha("BackGround Alpha", Float) = 1.0

        _Grid_Center("Grid Center", Vector) = (0, 0, 0, 0)

        [Header(Global Opacity Control)] [Space]
        _Global_Visibility("Global Visibility", Range(0, 1)) = 1.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+2"
            "RenderPipeline"="UniversalPipeline"
        }
        Pass
        {
            Name "DrawGrid"
        
            Cull Off
            Blend Off
            ZTest LEqual
            ZWrite Off
            
            // ZTest LEqual
            // ZWrite Off
            
            Stencil
            {
                Ref 1
                Comp NotEqual // Only passes if stencil != 1
                Pass Keep
            }

            HLSLPROGRAM
            
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            
            #define FULLSCREEN_SHADERGRAPH
            
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_VERTEXID
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_TEXCOORD1            
            #define REQUIRE_DEPTH_TEXTURE
            #define REQUIRE_NORMAL_TEXTURE
            #define SHADERPASS SHADERPASS_DRAWPROCEDURAL
            
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "./CustomBlendingFunctions.hlsl"
            
            TEXTURE2D(_GridMaskTex);   SAMPLER(sampler_GridMaskTex);

            struct Attributes
            {
                uint vertexID : VERTEXID_SEMANTIC;
            };
            
            struct SurfaceDescriptionInputs
            {
                float3 WorldSpacePosition;
                float4 ScreenPosition;
                float2 NDCPosition;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0;
                float4 texCoord1;
            };

            struct VertexDescriptionInputs
            {
            };

            struct PackedVaryings
            {
                float4 positionCS : SV_POSITION;
                float4 texCoord0 : INTERP0;
                float4 texCoord1 : INTERP1;
            };

            struct SurfaceDescription
            {
                float3 BaseColor;
                float Alpha;
            };
        
            PackedVaryings PackVaryings(Varyings input)
            {
                PackedVaryings output;
                ZERO_INITIALIZE(PackedVaryings, output);

                output.positionCS = input.positionCS;
                output.texCoord0.xyzw = input.texCoord0;
                output.texCoord1.xyzw = input.texCoord1;
                
                return output;
            }
            
            Varyings UnpackVaryings(PackedVaryings input)
            {
                Varyings output;

                output.positionCS = input.positionCS;
                output.texCoord0 = input.texCoord0.xyzw;
                output.texCoord1 = input.texCoord1.xyzw;
                
                return output;
            }
            
            CBUFFER_START(UnityPerMaterial)

            half4 _GridColor;
            half4 _GridLitColor;
            half4 _BackgroundColor;
            float _CellSize;
            float _ArmWidth;
            float _ArmLength;

            float _HeightCutoff;
            float _HeightCutoffFeather;

            float _ScanEffectRadius;
            float _ScanEffectRingWidth;

            float _EdgeGridRadius;
            float _EdgeGridWidth;
            float _EdgeGridInnerFeather;
            float _EdgeGridOuterFeather;

            float3 _MaskCenter;
            float _GridMaskRadius;
            float _GridMaskFeather;
            float _GridAlpha;
            
            float _GridMaskInnerRadius;
            float _GridMaskInnerFeather;

            float _BackgroundMaskRadius;
            float _BackgroundMaskFeather;
            float _BackgroundAlpha;

            float _BackgroundMaskInnerRadius;
            float _BackgroundMaskInnerFeather;

            // Terrain Scan Main Variables.
            float3 _Grid_Center;
            float _Global_Visibility;

            CBUFFER_END        
        
            // Global property required for fullscreen effects.       
            float _FlipY;
            
            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 SampleColorTexture(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
            }

            float2 Remap(float2 inputValue, float2 inputMinMax, float2 outputMinMax)
            {
                float remapMultiplier = (outputMinMax.y - outputMinMax.x) / (inputMinMax.y - inputMinMax.x);
                return outputMinMax.xx + (inputValue - inputMinMax.xx) * remapMultiplier.xx;
            }


            float3 CalculateWorldPosition(float2 screenPosition, float rawDepth)
            {
                // Remapping from [0, 1] range into [-1, 1] range.
                float2 remappedScreenPosition = Remap(screenPosition, float2(0, 1), float2(-1, 1));
                // Flipping Y coordinate to match correct Y direction.
                remappedScreenPosition.y *= -1.0;

                // Transformation from clip space into view space with inverse projection matrix.
                float4 clipSpacePosition = float4(remappedScreenPosition, rawDepth, 1.0);
                float4 viewSpacePosition = mul(UNITY_MATRIX_I_P, clipSpacePosition);
                viewSpacePosition /= viewSpacePosition.w;

                // Getting the world position of the pixel.
                float3 worldSpacePosition = GetAbsolutePositionWS(TransformViewToWorld(viewSpacePosition.xyz));
                return worldSpacePosition;
            }

            float3 CalculateWorldPosition(float2 screenPosition)
            {
                float rawDepth = SampleSceneDepth(screenPosition);
                float3 worldSpacePosition = CalculateWorldPosition(screenPosition, rawDepth);
                return worldSpacePosition;
            }

            float InverseLerp(float minValue, float maxValue, float value)
            {
                return saturate((value - minValue) / (maxValue - minValue));
            }

            float CalculateCircularMask(float3 distanceFromCenter, float maskRadius, float maskFeather, float invertMask = 0)
            {
                // Calculate distance from center (ignore Y axis for ground plane)
                //float distanceFromCenter = length(worldToCenter.xz);
                
                // Create smooth mask with feathering
                float mask = smoothstep(maskRadius + maskFeather, maskRadius, distanceFromCenter);
                
                // Invert if needed
                if (invertMask > 0.5)
                    mask = 1.0 - mask;
                    
                return mask;
            }

            // For 3D objects - ring mask in world space
            float CalculateRingMask(float3 distanceFromCenter, float innerRadius, float outerRadius, float innerFeather = 0.1, float outerFeather = 0.1)
            {
                
                float innerEdge = smoothstep(innerRadius - innerFeather, innerRadius + innerFeather, distanceFromCenter);
                float outerEdge = 1.0 - smoothstep(outerRadius - outerFeather, outerRadius + outerFeather, distanceFromCenter);
    
                return innerEdge * outerEdge;
            }

            float3 TerrainScanFragmentFunction(SurfaceDescriptionInputs IN){

                // Screen UV in range [0, 1].
                float2 screenPosition = IN.NDCPosition.xy;
                float3 originalColor = SampleColorTexture(screenPosition).xyz;

                // --- Calculate world position of the pixel from its depth. ---
                float rawDepth = SampleSceneDepth(screenPosition);
                float3 worldSpacePosition = CalculateWorldPosition(screenPosition, rawDepth);

                if(worldSpacePosition.y > _HeightCutoff)
                {
                    return originalColor.xyz;    
                }

                // Making sure that the skybox doesn't get into calculations.
                if(rawDepth <= 0.0)
                {
                    return originalColor.xyz;
                }                                            

                // -- Calculate distance from the origin of the terrain scan effect. ---
                float rawDistanceFromOrigin = distance(_Grid_Center, worldSpacePosition);  

                //Grid

                // World-space grid coordinates (in cells)
                float2 gridUV = worldSpacePosition.xz / _CellSize;

                // Get distance to nearest integer grid intersection
                float2 nearest = round(gridUV);
                float2 dist = abs(gridUV - nearest); // distance from pixel to intersection center

                // Parameters
                float armHalfLength = 0.05;   // Half length of each plus arm, in grid cell units
                float armThickness  = 0.008;  // Thickness of arms, in grid cell units

                // Vertical arm: near center in X, extended in Z
                float verticalArm = step(dist.x, _ArmWidth) * step(dist.y, _ArmLength);

                // Horizontal arm: near center in Z, extended in X
                float horizontalArm = step(dist.y, _ArmWidth) * step(dist.x, _ArmLength);

                // Combine into a plus
                float plusShape = max(verticalArm, horizontalArm);

                // Anti-aliasing (optional, smooth edges)
                float feather = 0.01;
                float verticalAA = smoothstep(_ArmWidth + feather, _ArmWidth, dist.x) 
                                 * smoothstep(_ArmLength + feather, _ArmLength, dist.y);
                float horizontalAA = smoothstep(_ArmWidth + feather, _ArmWidth, dist.y) 
                                   * smoothstep(_ArmLength + feather, _ArmLength, dist.x);
                float plusAA = max(verticalAA, horizontalAA);

                // Blend with scene
                float alpha = saturate(plusAA);

                //Composite effect with masks
                float gridMask = CalculateCircularMask(rawDistanceFromOrigin, _GridMaskRadius, _GridMaskFeather);
                
                //Make Inner Radius Mask
                float centerMask = CalculateCircularMask(rawDistanceFromOrigin, _GridMaskInnerRadius, _GridMaskInnerFeather);
                gridMask = gridMask * (1 - centerMask) * alpha;

                float centerBackgroundMask = CalculateCircularMask(rawDistanceFromOrigin, _BackgroundMaskInnerRadius, _BackgroundMaskInnerFeather);

                float edgeRingMask = CalculateRingMask(rawDistanceFromOrigin, _EdgeGridRadius, _EdgeGridRadius + _EdgeGridWidth, _EdgeGridInnerFeather, _EdgeGridOuterFeather) * alpha;
                float edgeRingBackgroundMask = CalculateRingMask(rawDistanceFromOrigin, _EdgeGridRadius, _EdgeGridRadius + _EdgeGridWidth, _EdgeGridInnerFeather, _EdgeGridOuterFeather);


                float backGroundMask = CalculateCircularMask(rawDistanceFromOrigin, _BackgroundMaskRadius, _BackgroundMaskFeather);
                backGroundMask = backGroundMask * (1 - centerBackgroundMask) * _BackgroundAlpha;

                float heightMask = smoothstep(_HeightCutoff, _HeightCutoff - _HeightCutoffFeather, worldSpacePosition.y);
                float ringMask = CalculateRingMask(rawDistanceFromOrigin, _ScanEffectRadius, _ScanEffectRadius + _ScanEffectRingWidth);

                float3 gridBackgroundColor = lerp(originalColor, _BackgroundColor, backGroundMask);

                float3 gridColor = lerp(originalColor, _GridColor.rgb, gridMask);
                float3 litGridColor = lerp(gridColor, _GridLitColor.rgb, gridMask * ringMask);
                float3 gridResult = lerp(gridBackgroundColor, litGridColor, gridMask * _GridAlpha);


                float3 edgeGridColor = lerp(gridResult, _GridColor.rgb, edgeRingMask);
                float3 litEdgeGridColor = lerp(edgeGridColor, _GridLitColor.rgb, edgeRingMask * ringMask);

                float3 gridEdgeBackgroundColor = lerp(gridResult, _BackgroundColor, edgeRingBackgroundMask * _BackgroundAlpha);
                float3 edgeGridResult = lerp(gridEdgeBackgroundColor, litEdgeGridColor, edgeRingMask * _GridAlpha);


                //float3 finalGridResult = lerp(gridResult, edgeGridResult, edgeRingMask);

                float3 finalColor = lerp(originalColor, edgeGridResult, heightMask * _Global_Visibility);

                return finalColor;
            }
        
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                surface.Alpha = 1.0;
                surface.BaseColor = TerrainScanFragmentFunction(IN);
                return surface;                
            }
            
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);      

                float3 viewDirWS = normalize(input.texCoord1.xyz);
                float linearDepth = LinearEyeDepth(SampleSceneDepth(input.texCoord0.xy), _ZBufferParams);
                float3 cameraForward = -UNITY_MATRIX_V[2].xyz;
                float cameraDistance = linearDepth / dot(viewDirWS, cameraForward);
                float3 positionWS = viewDirWS * cameraDistance + GetCameraPositionWS();            
            
                output.WorldSpacePosition = positionWS;
                output.ScreenPosition = float4(input.texCoord0.xy, 0, 1);
                output.NDCPosition = input.texCoord0.xy;
            
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
            
                return output;
            }
            
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/Fullscreen/Includes/FullscreenCommon.hlsl"
            #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/Fullscreen/Includes/FullscreenDrawProcedural.hlsl"
        
            ENDHLSL
        }
    }
    CustomEditor "UnityEditor.Rendering.Fullscreen.ShaderGraph.FullscreenShaderGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}