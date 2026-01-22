Shader "Custom/SeismicSonar"
{
        Properties
    {
        _RingColor ("Ring Color", Color) = (1, 1, 1, 1)

        _RingRadius ("Ring Radius",Vector) = (1, 1, 1, 1)
        _RingWidth ("Ring Width", Float) = 0.22
        _RingFeather ("Ring Feather", Float) = 1
        _RingOuterFeather ("Ring Outer Feather", Float) = 1
    
        _CircleRadius("Circle Radius", Float) = 5.0
        _CircleFeather("Circle Feather", Float) = 1.0

        _Rotation("Line Rotation", Float) = 1
        _LineLength("Line Length", Float) = 1
        _LineCount("Line Count", Int) = 1
        _LineThickness("Line Thickness", Float) = 1
        _LineSmoothness("Line Smoothness", Float) = 1
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
            float4 _RingRadius;
            float _RingWidth;
            float _RingFeather;
            float _RingOuterFeather;

            float _CircleRadius;
            float _CircleFeather;

            float _Rotation;
            float _LineLength;
            int _LineCount;
            float _LineThickness;
            float _LineSmoothness;


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
               
                OUT.uv = IN.uv * 2 - 1;

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

            float CalculateCircularMask(float3 worldToCenter, float maskRadius, float maskFeather, float invertMask = 0)
            {
                // Calculate distance from center (ignore Y axis for ground plane)
                float distanceFromCenter = length(worldToCenter.xz);
                
                // Create smooth mask with feathering
                float mask = smoothstep(maskRadius + maskFeather, maskRadius, distanceFromCenter);
                
                // Invert if needed
                if (invertMask > 0.5)
                    mask = 1.0 - mask;
                    
                return mask;
            }

            // Single radial line SDF
            float DrawRadialLine(float2 p, float angle, float len, float thick)
            {
                // Rotate point to line's coordinate space
                float c = cos(angle), s = sin(angle);
                float2 q = float2(p.x * c + p.y * s, -p.x * s + p.y * c);
                
                // Distance to line segment from (0,0) to (len, 0)
                float2 a = float2(0, 0);
                float2 b = float2(len, 0);
                float2 pa = q - a;
                float2 ba = b - a;
                float h = saturate(dot(pa, ba) / dot(ba, ba));
                float d = length(pa - ba * h);
                
                return d - thick * 0.5;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float lineMask = 0;
                
                // Draw multiple lines radiating from center
                for(int n = 0; n < _LineCount; n++)
                {
                    float angle = (float)n / _LineCount * 6.28318 + _Rotation * 0.01745329252;
                    float d = DrawRadialLine(uv, angle, _LineLength, _LineThickness);
                    float lineSegment = 1.0 - smoothstep(0, _LineSmoothness, d);
                    lineMask = max(lineMask, lineSegment);
                }

               float ringOneMask = CalculateRingMask(IN.localPosition, _RingRadius.x, _RingRadius.x + _RingWidth, _RingFeather, _RingOuterFeather);
               float ringTwoMask = CalculateRingMask(IN.localPosition, _RingRadius.y, _RingRadius.y + _RingWidth, _RingFeather, _RingOuterFeather);
               float circleMask = CalculateCircularMask(IN.localPosition, _CircleRadius, _CircleFeather);

               half4 lineColor = lerp((0, 0, 0, 0), _RingColor, lineMask);
               half4 indicatorColor = lerp(lineColor, (0, 0, 0, 0), circleMask);
               half4 ringColor = lerp(indicatorColor, _RingColor, ringOneMask + ringTwoMask);

               return ringColor;
            }
            ENDHLSL
        }
    }
}
