Shader "Custom/SonarBlipShader"
{
    Properties
    {
        _BlockColor ("Block Color", Color) = (0,1,1,1)
        _CenterAngle ("Center Angle", Float) = 0
        _MinRadius ("Min Radius", Float) = 5
        _MaxRadius ("Max Radius", Float) = 25
        _BlipSize ("Blip Size", Float) = .1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // #define PI 3.14159265359
            // #define TWO_PI 6.28318530718
            // #define HALF_PI 1.57079632679
            // #define INV_PI 0.31830988618

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 displayUV : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)

            half4 _BlockColor;

            float _MinRadius, _MaxRadius;
            float _AngleHalfWidth;
            float _CenterAngle;
            float4x4 _DisplayMatrix;
            float4x4 _DisplayMatrixInv;
            float _BlipSize;

            CBUFFER_END

            StructuredBuffer<float2> _BlipDataBuffer;
            

            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                float2 blip = _BlipDataBuffer[instanceID];
                float angle = blip.x;
                float radius = blip.y;
                
                float angleDelta = angle - _CenterAngle;
                angleDelta = fmod(angleDelta + PI, TWO_PI) - PI;
                
                float u = (angleDelta / _AngleHalfWidth) * 0.5 + 0.5;
                float v = (radius - _MinRadius) / (_MaxRadius - _MinRadius);

                float3 localPos = float3(u - 0.5, v - 0.5, 0);
                localPos.xy += IN.positionOS.xy * _BlipSize;
                localPos.z -= 0.002;
                float4 worldPos = mul(_DisplayMatrix, float4(localPos, 1.0));

                float2 blipSizeUV = float2(_BlipSize / (_AngleHalfWidth * 2), _BlipSize / (_MaxRadius - _MinRadius));

                float2 uvOffset = IN.positionOS.xy * blipSizeUV;
                float2 displayUV = float2(u, v) + uvOffset;

                Varyings OUT;
                OUT.positionHCS = mul(UNITY_MATRIX_VP, worldPos);
                OUT.uv = IN.uv;
                OUT.worldPos = worldPos.xyz;
                OUT.displayUV = displayUV;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {   
                clip(IN.displayUV.x);
                clip(1.0 - IN.displayUV.x);
                clip(IN.displayUV.y);
                clip(1.0 - IN.displayUV.y);

                half4 color = _BlockColor;
    
                return color;
            }
            ENDHLSL
        }
    }
}
