Shader "Custom/SonarPingPong"
{
    Properties
    {
        _BlockColor ("Block Color", Color) = (0,1,1,1)
        _BackgroundColor ("Background Color", Color) = (0,0,0,0)
        _OuterRadius ("Outer Radius", Float) = 1.0
        _InnerRadius ("Inner Radius", Float) = 0.5
        _PositionY ("Position Y", Float) = 0.5
        _Speed ("Speed", Float) = 0.25
        _Delta ("Delta", Float) = 0
        _HalfSize ("Half Size", Vector) = (0.05,0.05,0,0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

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
            };

            CBUFFER_START(UnityPerMaterial)

            half4 _BlockColor, _BackgroundColor;
            float  _OuterRadius, _InnerRadius, _Speed, _Delta, _PositionY;
            float2 _HalfSize;

            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv * 2.0 - 1.0;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 worldUV = IN.uv;

                float2 centre = float2(_OuterRadius, _PositionY);

                centre.x -= normalize(centre) * _Speed * _Delta;
                
                float2 d = abs(worldUV - centre) - _HalfSize;
                float dist = length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
                float mask = 1.0 - smoothstep(0.0, 0.005, dist);

                half4 color = lerp(_BackgroundColor, _BlockColor, mask);
                return color;
            }
            ENDHLSL
        }
    }
}
