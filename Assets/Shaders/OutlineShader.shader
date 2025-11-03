Shader "Custom/OutlineShader"
{   
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineThickness("Outline Thickness", Float) = 1
        _DepthMultiplier("Depth Multiplier", Float) = 1
        _DepthBias("Depth Bias", Float) = 1
        _NormalMultiplier("Normal Multiplier", Float) = 1
        _NormalBias("Normal Bias", Float) = 1
    }

    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Tags { "RenderType"="Opaque" }
        LOD 100
        //ZWrite Off Cull Off
        Pass
        {
            Name "OutlineShader"

            Cull Off
            Blend Off
            ZTest LEqual
            ZWrite Off
            
            // ZTest LEqual
            // ZWrite Off
            
            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)

                half4 _OutlineColor;
                float _OutlineThickness;
                float _DepthMultiplier;
                float _DepthBias;
                float _NormalMultiplier;
                float _NormalBias;

            CBUFFER_END

            TEXTURE2D(_CameraNormalsTexture);        
            SAMPLER(sampler_CameraNormalsTexture);

            float SobelDepthSample(float2 uv, float3 offset)
            {
                //float rawDepth = SampleSceneDepth(uv);
                float pixelCenter = LinearEyeDepth(SampleSceneDepth(uv).r, _ZBufferParams);
                float pixelLeft = LinearEyeDepth(SampleSceneDepth(uv - offset.xz).r, _ZBufferParams);
                float pixelRight = LinearEyeDepth(SampleSceneDepth(uv + offset.xz).r, _ZBufferParams);
                float pixelDown = LinearEyeDepth(SampleSceneDepth(uv - offset.zy).r, _ZBufferParams);
                float pixelUp = LinearEyeDepth(SampleSceneDepth(uv + offset.zy).r, _ZBufferParams);

                return abs(pixelLeft - pixelCenter) + abs(pixelRight - pixelCenter) + abs(pixelDown - pixelCenter) + abs(pixelUp - pixelCenter);
            }

            float4 SobelNormalSample(Texture2D t, SamplerState s, float2 uv, float3 offset)
            {
               
                float4 pixelCenter = t.Sample(s, uv);
                float4 pixelLeft   = t.Sample(s, uv - offset.xz);
                float4 pixelRight  = t.Sample(s, uv + offset.xz);
                float4 pixelUp     = t.Sample(s, uv + offset.zy);
                float4 pixelDown   = t.Sample(s, uv - offset.zy);
    
                return abs(pixelLeft  - pixelCenter) +
                       abs(pixelRight - pixelCenter) +
                       abs(pixelUp    - pixelCenter) +
                       abs(pixelDown  - pixelCenter);
            }

            float4 Frag (Varyings input) : SV_Target
            {
                float4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord).rgba;
                half4 outlineColor = lerp(sceneColor, _OutlineColor, _OutlineColor.a);

                //Depth
                float3 offset = float3((1.0 / _ScreenParams.x), (1.0 / _ScreenParams.y), 0.0) * _OutlineThickness;
                float sobelDepth = SobelDepthSample(input.texcoord, offset);
                
                //Normal
                float3 sobelNormalVectors = SobelNormalSample(_CameraNormalsTexture, sampler_CameraNormalsTexture, input.texcoord, offset).rgb;
                float sobelNormal = sobelNormalVectors.x + sobelNormalVectors.y + sobelNormalVectors.z;
                sobelNormal = pow(sobelNormal * _NormalMultiplier, _NormalBias);

                float sobelOutline = saturate(max(sobelDepth, sobelNormal));
            
                half4 color = lerp(sceneColor, outlineColor, sobelOutline);

                //half4 color = lerp(sceneColor, outlineColor, sobelDepth);

                return color;
            }
            
            ENDHLSL
        }
    }
}
