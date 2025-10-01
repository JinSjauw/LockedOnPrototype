Shader "Custom/GridShader"
{
    Properties
    {
       [HDR]_GridColor("Grid Color", Color) = (1,1,1,1)
       [HDR]_BackgroundColor("Background Color", Color) = (1,1,1,0)
       _CellSize("Cell Size", Float) = 1.0
       _LineWidth("Line Width", Float) = 0.05
       _LineHardness("Line Hardness", Float) = 1.5

       //_MaskCenter("Mask Center", Vector) = (0,0,0,0)
       _GridMaskRadius("Grid Mask Radius", Float) = 5.0
       _GridMaskFeather("Grid Mask Feather", Float) = 1.0
       //[Toggle]_InvertMask("Invert Mask", Float) = 0

       _BackgroundMaskRadius("Background Mask Radius", Float) = 5.0
       _BackgroundMaskFeather("Background Mask Feather", Float) = 1.0
       
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

           #pragma vertex vert
            #pragma fragment frag

            // Include URP core library
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

           half4 _GridColor;
           half4 _BackgroundColor;
           float _CellSize;
           float _LineWidth;
           float _LineHardness;

           float3 _MaskCenter;
           float _GridMaskRadius;
           float _GridMaskFeather;
            
           float _BackgroundMaskRadius;
           float _BackgroundMaskFeather;

           struct Attributes
           {
                float4 positionOS : POSITION;
           };

           struct Varyings
           {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 worldToCenter : TEXCOORD1;
           };

           Varyings vert(Attributes IN)
           {
                Varyings OUT;
                // Transform object space position to world space
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                // Transform position to clip space
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.worldToCenter = OUT.positionWS - _MaskCenter;

                return OUT;
           }

           // Function to calculate circular mask
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

           half4 frag(Varyings IN) : SV_Target
           {
                // Create grid coordinates in world space, adjusted by cell size
                float2 gridUV = IN.positionWS.xz / _CellSize;

                // Calculate anti-aliasing width using screen-space derivatives
                float2 lineAA = fwidth(gridUV) * _LineHardness;

                // Create a repeating triangle wave pattern (essential for the grid)
                float2 gridLines = 1.0 - abs(frac(gridUV) * 2.0 - 1.0);

                // Draw anti-aliased lines for both axes
                float2 grid = smoothstep(_LineWidth - lineAA, _LineWidth + lineAA, gridLines);

                // Combine the X and Y lines
                float finalGrid = max(grid.x, grid.y);

                float gridMask = CalculateCircularMask(IN.worldToCenter, _GridMaskRadius, _GridMaskFeather);
                float backGroundMask = CalculateCircularMask(IN.worldToCenter, _BackgroundMaskRadius, _BackgroundMaskFeather);

                //make it transparent?
                //Mix blackground or gridlines color with 0000 based on mask;

                half4 result;
                //_BackgroundColor.a *= backGroundMask;
                //_GridColor.a *= gridMask;
                
                half4 backGroundColor = lerp((0,0,0,0), _BackgroundColor, backGroundMask);
                half4 gridColor = lerp(backGroundColor, _GridColor, gridMask * finalGrid);
                
                //half4 finalColor = lerp(backGroundColor, gridColor, finalGrid);

                result = gridColor;
                //result.rgb = lerp(_BackgroundColor.rgb, _GridColor.rgb, finalGrid);

                // Apply the grid pattern to the color
                return result;
           }
           ENDHLSL
        }
    }
}
