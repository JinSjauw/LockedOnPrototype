Shader "Custom/SonarShader"
{
    Properties
    {
       [HDR]_MainColor("Main Color", Color) = (1,1,1,1)
       [HDR]_BackgroundColor("Background Color", Color) = (1,1,1,0)

       [HDR]_OccludedColor("Occluded Color", Color) = (1, 1, 1, 1)

       _Center("Center", Vector) = (0,0,0)
       _MaskRadius("Mask Radius", Float) = 5.0
       _MaskFeather("Mask Feather", Float) = 1.0
       _RingWidth("Ring Width", Float) = 1.0
       _WaveAmp("Ring Wave Amp", float) = 1.0
       _WaveCycles("Ring Wave Cycles", float) = 1.0
       //[Toggle]_InvertMask("Invert Mask", Float) = 0

       _BumpCount("Bump Count", int) = 5
       _BumpWorldPos("BumpPos", Vector) = (0, 0, 0)
       _BumpWidth("BumpWidth", float) = 2
       _BumpHeight("BumpHeight", float) = 4


       _BackgroundMaskRadius("Background Mask Radius", Float) = 5.0
       _BackgroundMaskFeather("Background Mask Feather", Float) = 1.0

       //_Grid_Center("Grid Center", Vector) = (0, 0, 0)
       
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
           ZWrite On
           Blend SrcAlpha OneMinusSrcAlpha

           HLSLPROGRAM

           #pragma vertex vert
           #pragma fragment frag

           // Include URP core library
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

           half4 _MainColor;
           half4 _BackgroundColor;
  
           float3 _Center;
           float _MaskRadius;
           float _MaskFeather;
            
           float _RingWidth;
           float _WaveAmp;
           float _WaveCycles;

           float3 _BumpWorldPos;
           float _BumpWidth;
           float _BumpHeight;

           float _BackgroundMaskRadius;
           float _BackgroundMaskFeather;

           //float3 _Grid_Center;

           struct Attributes
           {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
           };

           struct Varyings
           {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 worldToCenter : TEXCOORD2;
                float3 positionVS   : TEXCOORD3;
           };

           Varyings vert(Attributes IN)
           {
                Varyings OUT;
                // Transform object space position to world space
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                // Transform position to clip space
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                //Transform to ViewSpace
                OUT.positionVS = TransformWorldToView(OUT.positionWS);
                OUT.uv = IN.uv;
                OUT.worldToCenter = OUT.positionWS - _Center;

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

           float CalculateRingMask(float3 worldToCenter, float innerRadius, float outerRadius, float innerFeather, float outerFeather)
           {
               float distanceFromCenter = length(worldToCenter.xz);

               //float innerEdge = smoothstep(innerRadius - 0.1, innerRadius + 0.1, distanceFromCenter);
               float innerEdge = smoothstep(innerRadius - innerFeather, innerRadius + innerFeather, distanceFromCenter);
               float outerEdge = 1.0 - smoothstep(outerRadius - outerFeather, outerRadius + outerFeather, distanceFromCenter);
    
               return innerEdge * outerEdge;
           }

           float MakeTriangle(float x)
           {
                return 2.0 * abs(frac(x / (2.0 * PI) + 0.5) - 0.5);
           }

           float CalculateDeformedCircularMask(float3 worldToCenter,
                                    float baseRadius,
                                    float lineWidth,
                                    float waveAmp,     // how far the wave pushes
                                    float cycles,      
                                    float time)
           {
                // 1. polar coords
                float angle      = atan2(worldToCenter.z, worldToCenter.x);  // ‑π…π
                float distCenter = length(worldToCenter.xz);
                
                float wave = waveAmp * (sin(angle * cycles + time) * 0.2 + 0.2); // 0…1

                // float phase = angle * cycles + time;
                // float spike = MakeTriangle(phase);      //0 at valleys, 1 at peaks
                // float wave  = waveAmp * spike;

                // 2. deformed mid-radius
                float midRadius = baseRadius + wave;

                // 3. crisp thin line around that mid-radius
                return step(abs(distCenter - midRadius), lineWidth * 0.5);
           } 


           float SingleBumpRingMask(float3 worldPos,      // current pixel world pos
                         float3 ringCenter,    // world-space ring centre
                         float3 bumpWorldPos,  // world-space bump centre
                         float baseRadius,     // inner-most radius (fixed)
                         float lineWidth,      // thickness of the line
                         float bumpHeight,     // how far the bump pushes outward
                         float bumpWidth)      // angular half-width (radians)
           {
                float3 toCenter = worldPos - ringCenter;
                float  angle    = atan2(toCenter.z, toCenter.x);   // ‑π…π
                float  distCtr  = length(toCenter.xz);

                // angular distance to bump
                float3 toBump  = bumpWorldPos - ringCenter;
                float bumpAngle = atan2(toBump.z, toBump.x);
                float dAngle = abs(angle - bumpAngle);
                dAngle = min(dAngle, 2.0 * PI - dAngle);

                // outward-only bump (0…1)
                float bump = saturate(1.0 - dAngle / bumpWidth);
                bump = pow(bump, 2.0);

                //return bump * BumpHeight;

                // deformed mid-radius
                float midRadius = baseRadius + bump * bumpHeight;

                int pixelWidth = 8; //Modulate this based on how far the camera is with a min and max;

                // 3. world-size of one pixel at this distance from camera
                float pixelSize = abs(ddy(distCtr)) + abs(ddx(distCtr));   // ~1 pixel in world units
                float halfWidth = pixelWidth * pixelSize * 0.5;

                // 4. crisp band in world units
                return step(abs(distCtr - midRadius), halfWidth);

                // crisp thin ring
                //return step(abs(distCtr - midRadius), lineWidth * 0.5);
           }


           half4 frag(Varyings IN) : SV_Target
           {
               float ringMask = CalculateRingMask(IN.worldToCenter, _MaskRadius, _MaskRadius + _RingWidth, _MaskFeather, _BackgroundMaskRadius);

               float deformedRingMask = CalculateDeformedCircularMask(IN.worldToCenter, _MaskRadius, _RingWidth, _WaveAmp, _WaveCycles, _Time.y);

               float bumpRingMask = SingleBumpRingMask(IN.worldToCenter + _Center, _Center, _BumpWorldPos, _MaskRadius, _RingWidth * 2, _BumpHeight, _BumpWidth);

               float hideRingMask = CalculateCircularMask(IN.worldToCenter, _MaskRadius, .1);


               //float bumpedRing
               half4 ringBumpColor = lerp((0, 0, 0, 0), _BackgroundColor, bumpRingMask);
               half4 hideBumpRing = lerp(ringBumpColor, (0, 0, 0, 0), hideRingMask);
               half4 ringColor = lerp(hideBumpRing, _MainColor, ringMask);
               half4 ringWaveColor = lerp(ringColor, _MainColor, deformedRingMask);

               return ringWaveColor;
           }
           ENDHLSL
        }

        //X-Ray Pass

        Pass
        {
           Tags { "Queue" = "Overlay+500" }
           ZTest Greater     // <-- ignore depth
           Blend SrcAlpha OneMinusSrcAlpha

           HLSLPROGRAM

           #pragma vertex vert
           #pragma fragment frag

           // Include URP core library
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

           half4 _OccludedColor;
           half4 _BackgroundColor;
  
           float3 _Center;
           float _MaskRadius;
           float _MaskFeather;
            
           float _RingWidth;
           float _WaveAmp;
           float _WaveCycles;

           float3 _BumpWorldPos;
           float _BumpWidth;
           float _BumpHeight;

           float _BackgroundMaskRadius;
           float _BackgroundMaskFeather;

           //float3 _Grid_Center;

           struct Attributes
           {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
           };

           struct Varyings
           {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 worldToCenter : TEXCOORD2;
                float3 positionVS   : TEXCOORD3;
           };

           Varyings vert(Attributes IN)
           {
                Varyings OUT;
                // Transform object space position to world space
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                // Transform position to clip space
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                //Transform to ViewSpace
                OUT.positionVS = TransformWorldToView(OUT.positionWS);
                OUT.uv = IN.uv;
                OUT.worldToCenter = OUT.positionWS - _Center;

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

           float CalculateRingMask(float3 worldToCenter, float innerRadius, float outerRadius, float innerFeather, float outerFeather)
           {
               float distanceFromCenter = length(worldToCenter.xz);

               //float innerEdge = smoothstep(innerRadius - 0.1, innerRadius + 0.1, distanceFromCenter);
               float innerEdge = smoothstep(innerRadius - innerFeather, innerRadius + innerFeather, distanceFromCenter);
               float outerEdge = 1.0 - smoothstep(outerRadius - outerFeather, outerRadius + outerFeather, distanceFromCenter);
    
               return innerEdge * outerEdge;
           }

           float MakeTriangle(float x)
           {
                return 2.0 * abs(frac(x / (2.0 * PI) + 0.5) - 0.5);
           }

           float CalculateDeformedCircularMask(float3 worldToCenter,
                                    float baseRadius,
                                    float lineWidth,
                                    float waveAmp,     // how far the wave pushes
                                    float cycles,      
                                    float time)
           {
                // 1. polar coords
                float angle      = atan2(worldToCenter.z, worldToCenter.x);  // ‑π…π
                float distCenter = length(worldToCenter.xz);
                
                float wave = waveAmp * (sin(angle * cycles + time) * 0.2 + 0.2); // 0…1

                // float phase = angle * cycles + time;
                // float spike = MakeTriangle(phase);      //0 at valleys, 1 at peaks
                // float wave  = waveAmp * spike;

                // 2. deformed mid-radius
                float midRadius = baseRadius + wave;

                // 3. crisp thin line around that mid-radius
                return step(abs(distCenter - midRadius), lineWidth * 0.5);
           } 


           float SingleBumpRingMask(float3 worldPos,      // current pixel world pos
                         float3 ringCenter,    // world-space ring centre
                         float3 bumpWorldPos,  // world-space bump centre
                         float baseRadius,     // inner-most radius (fixed)
                         float lineWidth,      // thickness of the line
                         float bumpHeight,     // how far the bump pushes outward
                         float bumpWidth)      // angular half-width (radians)
           {
                float3 toCenter = worldPos - ringCenter;
                float  angle    = atan2(toCenter.z, toCenter.x);   // ‑π…π
                float  distCtr  = length(toCenter.xz);

                // angular distance to bump
                float3 toBump  = bumpWorldPos - ringCenter;
                float bumpAngle = atan2(toBump.z, toBump.x);
                float dAngle = abs(angle - bumpAngle);
                dAngle = min(dAngle, 2.0 * PI - dAngle);

                // outward-only bump (0…1)
                float bump = saturate(1.0 - dAngle / bumpWidth);
                bump = pow(bump, 2.0);

                //return bump * BumpHeight;

                // deformed mid-radius
                float midRadius = baseRadius + bump * bumpHeight;

                int pixelWidth = 8; //Modulate this based on how far the camera is with a min and max;

                // 3. world-size of one pixel at this distance from camera
                float pixelSize = abs(ddy(distCtr)) + abs(ddx(distCtr));   // ~1 pixel in world units
                float halfWidth = pixelWidth * pixelSize * 0.5;

                // 4. crisp band in world units
                return step(abs(distCtr - midRadius), halfWidth);

                // crisp thin ring
                //return step(abs(distCtr - midRadius), lineWidth * 0.5);
           }


           half4 frag(Varyings IN) : SV_Target
           {
               float ringMask = CalculateRingMask(IN.worldToCenter, _MaskRadius, _MaskRadius + _RingWidth, _MaskFeather, _BackgroundMaskRadius);

               float deformedRingMask = CalculateDeformedCircularMask(IN.worldToCenter, _MaskRadius, _RingWidth, _WaveAmp, _WaveCycles, _Time.y);

               float bumpRingMask = SingleBumpRingMask(IN.worldToCenter + _Center, _Center, _BumpWorldPos, _MaskRadius, _RingWidth * 2, _BumpHeight, _BumpWidth);

               float hideRingMask = CalculateCircularMask(IN.worldToCenter, _MaskRadius, .1);


               //float bumpedRing
               half4 ringBumpColor = lerp((0, 0, 0, 0), _OccludedColor, bumpRingMask);
               half4 hideBumpRing = lerp(ringBumpColor, (0, 0, 0, 0), hideRingMask);
               half4 ringColor = lerp(hideBumpRing, _OccludedColor, ringMask);
               half4 ringWaveColor = lerp(ringColor, _OccludedColor, deformedRingMask);

               return ringWaveColor;
           }
           ENDHLSL
        }
    }
}
