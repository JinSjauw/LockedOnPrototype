Shader "Custom/SonarShader"
{
    Properties
    {
       [HDR]_MainColor("Main Color", Color) = (1,1,1,1)
       [HDR]_BackgroundColor("Background Color", Color) = (1,1,1,0)

       [HDR]_WaveColorA("Wave Color A", Color) = (1,1,1,1)
       [HDR]_WaveColorB("Wave Color B", Color) = (1,1,1,1)
       [HDR]_WaveColorC("Wave Color C", Color) = (1,1,1,1)
       [HDR]_WaveColorD("Wave Color D", Color) = (1,1,1,1)

       _Center("Center", Vector) = (0,0,0)
       _RingRadius("Ring Radius", Float) = 5.0
       _RingFeather("Ring Feather", Float) = 1.0
       _RingOuterFeather("Ring Outer Feather", Float) = 5.0
       _RingWidth("Ring Width", Float) = 1.0

       _OuterRingRadius("Outer Ring Radius", Float) = 5.0
       _OuterRingFeather("Outer Ring Feather", Float) = 1.0
       _OuterRingWidth("Outer Ring Width", Float) = 1.0

       //0-3
       _WaveAmpsA("Amps", Vector) = (0.02, 0.01, 0.005, 0.002)
       _WaveFreqA("Freq", Vector) = (3, 7, 12, 25)
       _WavePhaseA("Phase", Vector) = (0, 1.2, -2.1, 0.7)

       //4-7
       _WaveAmpsB("Amps B", Vector) = (0.02, 0.01, 0.005, 0.002)
       _WaveFreqB("Freq B", Vector) = (3, 7, 12, 25)
       _WavePhaseB("Phase B", Vector) = (0, 1.2, -2.1, 0.7)

       _WaveRingRadius("Wave Ring Radius", Vector) = (1,1,1,1)
       _WaveLineThickness("Wave Line Thickness", Vector) = (1,1,1,1)
       _WaveMoveInterval("Wave Move Interval", Vector) = (1,1,1,1)

       _BumpCount("Bump Count", int) = 5
       _BumpWorldPosition("BumpPos", Vector) = (0, 0, 0)
       _BumpWidth("BumpWidth", float) = 2
       _BumpHeight("BumpHeight", float) = 4

       
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
           ZWrite Off
           Blend SrcAlpha OneMinusSrcAlpha

           HLSLPROGRAM

           #pragma vertex vert
           #pragma fragment frag

           // Include URP core library
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

           half4 _MainColor;
           half4 _BackgroundColor;
  
           half4 _WaveColorA;
           half4 _WaveColorB;
           half4 _WaveColorC;
           half4 _WaveColorD;

           float3 _Center;
           float _RingRadius;
           float _RingFeather;
           float _RingOuterFeather;
           float _RingWidth;

           float _OuterRingRadius;
           float _OuterRingFeather;
           float _OuterRingWidth;

           float4 _WaveAmpsA;     // xyzw = amp 0-3
           float4 _WaveFreqA;    // xyzw = freq 0-3
           float4 _WavePhaseA;   // xyzw = phase 0-3 (radians)

           float4 _WaveAmpsB;     // xyzw = amp 4-7
           float4 _WaveFreqB;    // xyzw = freq 4-7
           float4 _WavePhaseB;   // xyzw = phase 4-7 (radians)

           float4 _WaveRingRadius;   // xyzw = speed 0-4 (rad/s)
           float4 _WaveLineThickness;
           float4 _WaveMoveInterval;

           #define MAX_BUMPS 8

           int _BumpCount;
           float4 _BumpWorldPositions[MAX_BUMPS];   // xyz = target positions (world) w = bumpHeight
           float _BumpWidths;                 // angular half-width (rad)


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

           float SnapPhase(float t, float interval, float a, float b)
           {
               //float interval = .2;

               float stepT = floor(t /interval) * interval;
               return (uint(stepT / interval) & 1u) ? a : b;
           }

           float SingleWaveOffset(float angle, float amp, float freq, float interval, float phase, float time)
           {
                float x = angle * freq + SnapPhase(time, interval, phase, -phase);
                float s = MakeTriangle(x);

                return amp * ((s + 1.0) * 0.5 - 0.5);
                //return amp * sin(angle * freq + time * phase) * 0.2 + 0.2;
           }

           float CalculateCompositeWaveMask(float3 worldToCenter, float baseRadius, float lineWidth, float moveInterval, float waveAmpA, float waveFreqA, float wavePhaseA, float waveAmpB, float waveFreqB, float wavePhaseB, float time)
           {
               //Multiple waves
               float angle = atan2(worldToCenter.z, worldToCenter.x);
               float distance = length(worldToCenter.xz);

               //Wave Composite 1
               float waveOffset = 0.0;
               waveOffset += SingleWaveOffset(angle, waveAmpA, waveFreqA, moveInterval, wavePhaseA, time) + waveAmpA / 2;
               waveOffset += SingleWaveOffset(angle, waveAmpB, waveFreqB, moveInterval, wavePhaseB, time) + waveAmpB / 2;
               
               //float dc = (_WaveAmpsA.x + _WaveAmpsA.y) * 0.05;

               float midRadius = baseRadius + waveOffset;

               float waveCompMask = step(abs(distance - midRadius), lineWidth * 0.5);
           
               return waveCompMask;
           }

           float SingleBumpRingMask(float3 worldPos, float3 ringCenter, float3 bumpWorldPos, float baseRadius, float lineWidth, float bumpHeight, float bumpWidth)
           {
                float3 toCenter = worldPos - ringCenter;
                float  angle    = atan2(toCenter.z, toCenter.x);   // ‑π…π
                float  distanceToCenter  = length(toCenter.xz);

                // angular distance to bump
                float3 toBump  = bumpWorldPos - ringCenter;
                float bumpAngle = atan2(toBump.z, toBump.x);
                float dAngle = abs(angle - bumpAngle);
                dAngle = min(dAngle, 2.0 * PI - dAngle);

                // outward-only bump (0…1)
                float bump = saturate(1.0 - dAngle / bumpWidth);
                bump = pow(bump, 2.0);

                // deformed mid-radius
                float midRadius = baseRadius + bump * bumpHeight;

                int pixelWidth = 8; //Modulate this based on how far the camera is with a min and max;

                // 3. world-size of one pixel at this distance from camera
                float pixelSize = abs(ddy(distanceToCenter)) + abs(ddx(distanceToCenter));   // ~1 pixel in world units
                float halfWidth = pixelWidth * pixelSize * 0.5;

                // 4. crisp band in world units
                return step(abs(distanceToCenter - midRadius), halfWidth);

                // crisp thin ring
                //return step(abs(distCtr - midRadius), lineWidth * 0.5);
           }

           float SmoothMax(float a, float b, float k)   // k = smoothness (0.01-0.1)
           {
                return max(a, b) + k * exp(-abs(a - b) / k);
           }

           float MultiBumpRingMask(float3 worldPos, float3 ringCenter, float baseRadius, float lineWidth, float bumpWidth)
           {
                float3 toCenter = worldPos - ringCenter;
                float  angle    = atan2(toCenter.z, toCenter.x);  
                float  distanceToCenter  = length(toCenter.xz);

                float totalBump = 0.0;

                //Calculate All bumps
                UNITY_UNROLL
                for (int i = 0; i < _BumpCount; ++i)
                {
                    float2 toTarget = _BumpWorldPositions[i].xy - ringCenter.xz;
                    float targetAng = atan2(toTarget.y, toTarget.x);
                    float dAng      = abs(angle - targetAng);
                    dAng            = min(dAng, 2.0 * PI - dAng);

                    float bump = saturate(1.0 - dAng / bumpWidth);
                    bump       = pow(bump, 2.0);
                    bump *=  _BumpWorldPositions[i].w;

                    totalBump = SmoothMax(totalBump, bump, 0.05);
                }

                float midRadius = baseRadius + totalBump;

                int pixelWidth = 8; //Modulate this based on how far the camera is with a min and max;

                // 3. world-size of one pixel at this distance from camera
                float pixelSize = abs(ddy(distanceToCenter)) + abs(ddx(distanceToCenter));   // ~1 pixel in world units
                float halfWidth = pixelWidth * pixelSize * 0.5;

                return step(abs(distanceToCenter - midRadius), halfWidth);
           }


           half4 frag(Varyings IN) : SV_Target
           {
               float ringMask = CalculateRingMask(IN.worldToCenter, _RingRadius, _RingRadius + _RingWidth, _RingFeather, _RingOuterFeather);
               //float ringMask = CalculateRingMask(IN.worldToCenter, _RingRadius, _RingRadius + _RingWidth, _RingFeather, _OuterRingRadius);

               //Waves

               float waveCompMaskA = CalculateCompositeWaveMask(IN.worldToCenter, _WaveRingRadius.x, _WaveLineThickness.x, _WaveMoveInterval.x, _WaveAmpsA.x, _WaveFreqA.x, _WavePhaseA.x, _WaveAmpsA.y, _WaveFreqA.y, _WavePhaseA.y, _Time.y);
               float waveCompMaskB = CalculateCompositeWaveMask(IN.worldToCenter, _WaveRingRadius.y, _WaveLineThickness.y, _WaveMoveInterval.y, _WaveAmpsA.z, _WaveFreqA.z, _WavePhaseA.z, _WaveAmpsA.w, _WaveFreqA.w, _WavePhaseA.w, _Time.y);
               float waveCompMaskC = CalculateCompositeWaveMask(IN.worldToCenter, _WaveRingRadius.z, _WaveLineThickness.z, _WaveMoveInterval.z, _WaveAmpsB.x, _WaveFreqB.x, _WavePhaseB.x, _WaveAmpsB.y, _WaveFreqB.y, _WavePhaseB.y, _Time.y);
               float waveCompMaskD = CalculateCompositeWaveMask(IN.worldToCenter, _WaveRingRadius.w, _WaveLineThickness.w, _WaveMoveInterval.w, _WaveAmpsB.z, _WaveFreqB.z, _WavePhaseB.z, _WaveAmpsB.w, _WaveFreqB.w, _WavePhaseB.w, _Time.y);

               //Waves end

               float bumpRingMask = MultiBumpRingMask(IN.worldToCenter + _Center, _Center, _RingRadius - _RingWidth, _RingWidth, .22);

               float hideRingMask = CalculateCircularMask(IN.worldToCenter, _RingRadius + _RingWidth, .1);

               float outerRingMask = CalculateRingMask(IN.worldToCenter, _OuterRingRadius, _OuterRingRadius + _OuterRingWidth, _OuterRingFeather, .01);

               //float bumpedRing
               half4 ringBumpColor = lerp((0, 0, 0, 0), _BackgroundColor, bumpRingMask);
               half4 hiddenBumpRing = lerp(ringBumpColor, (0, 0, 0, 0), hideRingMask);

               half4 ringColor = lerp((0, 0, 0, 0), _MainColor, ringMask);
               half4 ringWaveColor = lerp(ringColor, _WaveColorB, waveCompMaskA);
               half4 ringWaveCompColor = lerp(ringWaveColor, _WaveColorA, waveCompMaskB);
               half4 ringWaveCompColorB = lerp(ringWaveCompColor, _WaveColorC, waveCompMaskC);
               half4 ringWaveCompColorC = lerp(ringWaveCompColorB, _WaveColorD, waveCompMaskD);

               half4 ringWaveFinalColor = lerp(ringWaveCompColorC, _MainColor, outerRingMask);
               half4 finalColor = lerp(ringWaveFinalColor, hiddenBumpRing, hiddenBumpRing.a);

               return finalColor;
           }
           ENDHLSL
        }
    }
}
