Shader "Custom/SonarSelectioRing"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"

        _LineThickness("Line Thickness", Float) = 1
        _EdgeFeather("Edge Feather", Float) = 0.05
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float _LineThickness;
            float _EdgeFeather;

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
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
               // 1. read mask texture  (white = figure, black = background)
               half raw = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).r;

               // 2. convert to signed distance  (0.5 is the theoretical edge)
               half dist = (raw - 0.5) * 20;

               // 3. global thickness  (positive = thicker, negative = thinner)
               dist -= _LineThickness;

               // 4. smooth step mask
               half alpha = saturate((dist + _EdgeFeather) / (2.0 * _EdgeFeather));

               // 5. output unlit color
               half4 col = _BaseColor;
               col.a *= alpha;
               return col;
            }
            ENDHLSL
        }
    }
}
