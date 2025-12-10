Shader "Custom/FullscreenBase"
{
    Properties
    {
        [HDR]_MainColor("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        Pass
        {
            Name "Draw"
        
            Cull Off
            Blend Off
            ZTest LEqual
            ZWrite Off
            
            // ZTest LEqual
            // ZWrite Off
            
            Stencil
            {
                Ref 0
                Comp Equal // Only passes if stencil != 1
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

            half4 _MainColor;

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

            float3 FragmentFunction(SurfaceDescriptionInputs IN){

                // Screen UV in range [0, 1].
                float2 screenPosition = IN.NDCPosition.xy;
                half4 originalColor = SampleColorTexture(screenPosition);

                // --- Calculate world position of the pixel from its depth. ---
                float rawDepth = SampleSceneDepth(screenPosition);
                float3 worldSpacePosition = CalculateWorldPosition(screenPosition, rawDepth);

                // Making sure that the skybox doesn't get into calculations.
                if(rawDepth <= 0.0)
                {
                    return originalColor;
                }                                            
                
                return originalColor * _MainColor;
            }
        
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                surface.Alpha = 1.0;
                surface.BaseColor = FragmentFunction(IN);
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