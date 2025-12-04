Shader "Custom/SonarSelectioRing"
{
    Properties
    {
        _RingColor ("Ring Color", Color) = (1, 1, 1, 1)

        _RingRadius ("Ring Radius", Float) = 5
        _RingWidth ("Ring Width", Float) = 0.22
        _RingFeather ("Ring Feather", Float) = 1
        _RingOuterFeather ("Ring Outer Feather", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            //Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            half4 _RingColor;
            float _RingRadius;
            float _RingWidth;
            float _RingFeather;
            float _RingOuterFeather;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 localPosition : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.localPosition = IN.positionOS.xyz;
               
                OUT.uv = IN.uv;

                return OUT;
            }

            float CalculateRingMask(float3 toCenter, float innerRadius, float outerRadius, float innerFeather, float outerFeather)
            {
               float distanceFromCenter = length(toCenter.xz);

               //float innerEdge = smoothstep(innerRadius - 0.1, innerRadius + 0.1, distanceFromCenter);
               float innerEdge = smoothstep(innerRadius - innerFeather, innerRadius + innerFeather, distanceFromCenter);
               float outerEdge = 1.0 - smoothstep(outerRadius - outerFeather, outerRadius + outerFeather, distanceFromCenter);
    
               return innerEdge * outerEdge;
            }

            half4 frag(Varyings IN) : SV_Target
            {
               float ringMask = CalculateRingMask(IN.localPosition, _RingRadius, _RingRadius + _RingWidth, _RingFeather, _RingOuterFeather);

               half4 ringColor = lerp((0, 0, 0, 0), _RingColor, ringMask);

               return ringColor;
            }
            ENDHLSL
        }
    }
}
