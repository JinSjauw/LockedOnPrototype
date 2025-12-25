Shader "Custom/KalmanPoint"
{
    Properties
    {
        [HDR][MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [HDR] _FlashColor("Flash Color", Color) = (1, 1, 1, 1)
        _FlashDuration("Flash Duration", Float) = 0.1
        //[MainTexture] _BaseMap("Base Map", 2D) = "white"
        _QuadSize("Quad Size", Float) = 1
        _StencilRefID("Stencil Ref ID", Int) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Stencil
            {
                Ref   [_StencilRefID]
                Comp  Always
                Pass  Replace        // write value
            }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                uint id            : TEXCOORD3;
            };

            // TEXTURE2D(_BaseMap);
            // SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float _QuadSize;

            float4 _TerrainOrigin;
            float4 _TerrainSize;
            
            float _FlashDuration;

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _FlashColor;
                //float4 _BaseMap_ST;
                StructuredBuffer<float4> _Positions;
                StructuredBuffer<float> _StartTime;
                int _StencilRefID;
            CBUFFER_END

            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;

                float3 center = _Positions[instanceID];
                float2 uv = (center.xz - _TerrainOrigin.xz) / _TerrainSize.xz;

                float3 rawNormal = _NormalMap.SampleLevel(sampler_NormalMap, uv, 0).xyz;

                float3 normal = normalize(rawNormal * 2.0 - 1.0); 

                float3 forward = float3(0, 1, 1);

                float3 tangent = normalize(forward - normal * dot(forward, normal));
                float3 biTangent = cross(normal, tangent);

                float3x3 TBN = float3x3(biTangent, tangent, normal);

                float3 localPos = IN.positionOS.xyz * _QuadSize;
                float3 worldPos = center + mul(TBN, localPos);

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldPosition = worldPos;
                OUT.worldNormal = normal;
                OUT.uv = IN.uv;

                OUT.id = instanceID;
             
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                //half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor; 
                float age = _Time.y - _StartTime[IN.id];
                float flashAlpha = (age - 0) / (_FlashDuration - 0);
               
                flashAlpha = saturate(flashAlpha);

                half4 finalColour = lerp(_FlashColor, _BaseColor, flashAlpha);
                
                return finalColour;
            }
            ENDHLSL
        }
    }
}
