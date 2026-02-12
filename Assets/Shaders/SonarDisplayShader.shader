Shader "Custom/SonarDisplayShader"
{
    Properties
    {
        _BlockColor ("Block Color", Color) = (0,1,1,1)
        _BackgroundColor ("Background Color", Color) = (0,0,0,0)
        _OuterRadius ("Outer Radius", Float) = 1.0
        _InnerRadius ("Inner Radius", Float) = 0.5
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            struct BlockData
            {
                float2 position;
                float2 size;
                float hitIndex;
            };

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

            half4 _BlockColor, _BlockHitColor, _BackgroundColor;
            float  _OuterRadius, _InnerRadius, _Speed, _Delta, _PositionY;
            float2 _HalfSize;

            CBUFFER_END

            StructuredBuffer<BlockData> _BlockDataBuffer;
            int _BlockCount;

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

                float blockCompositeMask = 0.0;

                for(int i = 0; i < _BlockCount; i++)
                {
                    BlockData block = _BlockDataBuffer[i];
                    
                    if(block.hitIndex > 0) continue;

                    float2 d = abs(worldUV - block.position) - block.size;
                    float dist = length(max(d, 0.0)) + min(max(d.x, d.y), 0);
                    float mask = 1.0 - smoothstep(0.0, 0.005, dist);

                    blockCompositeMask = max(blockCompositeMask, mask * _BlockColor.a);
                }

                half4 color = lerp(_BackgroundColor, _BlockColor, blockCompositeMask);

                return color;
            }
            ENDHLSL
        }
    }
}
