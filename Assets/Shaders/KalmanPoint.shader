Shader "Custom/KalmanPoint"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            StructuredBuffer<float4> _Positions;
            StructuredBuffer<float4> _Normals;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float2 uv          : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float4 _TerrainOrigin;
            float4 _TerrainSize;


            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;

                float3 center = _Positions[instanceID];
                float2 uv = (center.xz - _TerrainOrigin.xz) / _TerrainSize.xz;

                float3 rawNormal = _NormalMap.SampleLevel(sampler_NormalMap, uv, 0).xyz;

                rawNormal.x;
                rawNormal.z;

                float3 normal = normalize(rawNormal * 2.0 - 1.0); 

                //float3 forward = abs(normal.y) < 0.99 ? float3(0, 1, 0) : float3(0, 0, 1);
                
                //Right works sometimes
                //Up axes works sometimes aswell
                //forward is the most stable

                float3 forward = float3(0, 1, 2);

                float3 tangent = normalize(forward - normal * dot(forward, normal));
                float3 biTangent = cross(normal, tangent);

                float3x3 TBN = float3x3(biTangent, tangent, normal);

                float3 localPos = IN.positionOS.xyz * .5;
                float3 worldPos = center + mul(TBN, localPos);

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldNormal = normal;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor; 
                return color;
            }
            ENDHLSL
        }
    }
}
