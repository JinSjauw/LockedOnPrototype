Shader "Custom/KalmanPoint"
{
    Properties
    {
        [HDR][MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
        _QuadSize("Quad Size", Float) = 1
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

            float _QuadSize;

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
                StructuredBuffer<float4> _Positions;
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

                //Calculate world locked UV's
                //Select world axes to lock on
                float3 worldU = float3(1, 0, 0); // world X
                float3 worldV = float3(0, 0, 1); // world Z

                // Project onto the surface plane
                float3 uDir = worldU - normal * dot(worldU, normal);
                float3 vDir = worldV - normal * dot(worldV, normal);

                // Handle near-parallel cases
                uDir = normalize(uDir);
                vDir = normalize(vDir);
                vDir = normalize(cross(normal, uDir));

                float3 delta = worldPos - center;
                OUT.uv.x = dot(delta, uDir) / _QuadSize + 0.5;
                OUT.uv.y = dot(delta, vDir) / _QuadSize + 0.5;

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldPosition = worldPos;
                OUT.worldNormal = normal;
             
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
