Shader "Custom/SonarShader"
{
    Properties
    {
       [HDR]_RingColour("Main Colour", Color) = (1,1,1,1)
       [HDR]_BackGroundColour("Background Colour", Color) = (1,1,1,1)
       
       [HDR]_WaveColourA("Wave Colour A", Color) = (1,1,1,1)
       [HDR]_WaveColourB("Wave Colour B", Color) = (1,1,1,1)
       [HDR]_WaveColourC("Wave Colour C", Color) = (1,1,1,1)
       [HDR]_WaveColourD("Wave Colour D", Color) = (1,1,1,1)

       _PixelWidthSonar("Pixel Width Sonar", Vector) = (1, 1, 1, 1)

       _Center("Center", Vector) = (0,0,0)

       _RingRadius("Ring Radius", Float) = 5.0
       _RingFeather("Ring Feather", Float) = 1.0
       _RingOuterFeather("Ring Outer Feather", Float) = 5.0
       _RingWidth("Ring Width", Float) = 1.0

       _RingBackgroundRadius("Ring Background Radius", Float) = 5.0
       _RingBackgroundFeather("Ring Background Feather", Float) = 1.0
       _RingBackgroundOuterFeather("Ring Background Outer Feather", Float) = 5.0
       _RingBackgroundWidth("Ring Background Width", Float) = 1.0

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

           half4 _RingColour;
           half4 _BackGroundColour;
           half4 _SpikeColour;
  
           half4 _WaveColourA;
           half4 _WaveColourB;
           half4 _WaveColourC;
           half4 _WaveColourD;

           float4 _PixelWidthSonar; //xy = min, max spike pixels, zw = min, max sonarWaves
           float _Zoom;

           float3 _Center;

           float _RingRadius;
           float _RingFeather;
           float _RingOuterFeather;
           float _RingWidth;

           float _RingBackgroundRadius;
           float _RingBackgroundFeather;
           float _RingBackgroundOuterFeather;
           float _RingBackgroundWidth;

           float4 _WaveAmpsA;     // xyzw = amp 0-3
           float4 _WaveFreqA;    // xyzw = freq 0-3
           float4 _WavePhaseA;   // xyzw = phase 0-3 (radians)

           float4 _WaveAmpsB;     // xyzw = amp 4-7
           float4 _WaveFreqB;    // xyzw = freq 4-7
           float4 _WavePhaseB;   // xyzw = phase 4-7 (radians)

           float4 _WaveRingRadius;   // xyzw = speed 0-4 (rad/s)
           float4 _WaveLineThickness;
           float4 _WaveMoveInterval;

           #define MAX_Spikes 8
           #define MAX_LOCK_COLOURS 4

           int _SpikeCount;
           float4 _SpikeData[MAX_Spikes];   // xy = targetPositions z = spikeWidth w = spikeHeight
           float _SpikesColourIndex[MAX_Spikes];
           float4 _SpikeColours[MAX_LOCK_COLOURS]; 
           half4 _SpikeMaskColour;

           float4 _WaveAlphas;

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

           float CalculateRingMask(float3 worldToCenter, float innerRadius, float outerRadius, float innerFeather, float outerFeather, bool dashed = false)
           {
               float distanceFromCenter = length(worldToCenter.xz);

               //float innerEdge = smoothstep(innerRadius - 0.1, innerRadius + 0.1, distanceFromCenter);
               float innerEdge = smoothstep(innerRadius - innerFeather, innerRadius + innerFeather, distanceFromCenter);
               float outerEdge = 1.0 - smoothstep(outerRadius - outerFeather, outerRadius + outerFeather, distanceFromCenter);
    
               float ring = innerEdge * outerEdge;

               if(!dashed) return ring;

               float ang = atan2(worldToCenter.z, worldToCenter.x) * 0.5 + 0.5; // 0-1
               float dash = frac(ang * 12);
               float on = step(dash, 0.6);

               return ring * on;
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

               float midRadius = baseRadius + waveOffset;

               //Use multiplier here
               float distanceToCenter = length(worldToCenter.xz);
               float pixelSize = abs(ddy(distanceToCenter)) + abs(ddx(distanceToCenter));
               float pixelScalar = lerp(_PixelWidthSonar.z, _PixelWidthSonar.w, _Zoom);

               float waveCompMask = step(abs(distance - midRadius), (lineWidth * pixelScalar) * pixelSize * 0.5);
           
               return waveCompMask;
           }

           float SmoothMax(float a, float b, float k)   // k = smoothness (0.01-0.1)
           {
                return max(a, b) + k * exp(-abs(a - b) / k);
           }

           float MultiSpikeRingMask(float3 worldPos, float3 ringCenter, float baseRadius, float lineWidth)
           {
                float3 toCenter = worldPos - ringCenter;
                float  angle    = atan2(toCenter.z, toCenter.x);  
                float  distanceToCenter  = length(toCenter.xz);

                float totalSpike = 0.0;
                half4 SpikeColour = 0;


                //Calculate All Spikes
                UNITY_UNROLL
                for (int i = 0; i < _SpikeCount; ++i)
                {
                    float2 toTarget = _SpikeData[i].xy - ringCenter.xz;
                    float targetAng = atan2(toTarget.y, toTarget.x);
                    float dAng      = abs(angle - targetAng);
                    dAng            = min(dAng, 2.0 * PI - dAng);

                    float Spike = saturate(1.0 - dAng / _SpikeData[i].z);
                    Spike       = pow(Spike, 2.0);
                    Spike *=  _SpikeData[i].w;

                    totalSpike = SmoothMax(totalSpike, Spike, 0.05);

                    float heightMask = step(Spike, _SpikeData[i].w);

                    float peakBias = 0.75;
                    float spikeWeight = saturate(pow(abs(Spike), peakBias)) * heightMask;

                    SpikeColour = lerp(SpikeColour, _SpikeColours[_SpikesColourIndex[i]], spikeWeight);
                }

                float midRadius = baseRadius + totalSpike;

                int pixelWidth = lerp(_PixelWidthSonar.x, _PixelWidthSonar.y, _Zoom); //Modulate this based on how far the camera is with a min and max;

                // 3. world-size of one pixel at this distance from camera
                float pixelSize = abs(ddy(distanceToCenter)) + abs(ddx(distanceToCenter));   // ~1 pixel in world units
                float halfWidth = pixelWidth * pixelSize * 0.5;

                _SpikeMaskColour = SpikeColour;
                return step(abs(distanceToCenter - midRadius), halfWidth);
           }


           half4 frag(Varyings IN) : SV_Target
           {
               float ringMask = CalculateRingMask(IN.worldToCenter, _RingRadius, _RingRadius + _RingWidth, _RingFeather, _RingOuterFeather);
               float ringBackgroundMask = CalculateRingMask(IN.worldToCenter, _RingBackgroundRadius, _RingBackgroundRadius + _RingBackgroundWidth, _RingBackgroundFeather, _RingBackgroundOuterFeather);
               //float ringMask = CalculateRingMask(IN.worldToCenter, _RingRadius, _RingRadius + _RingWidth, _RingFeather, _OuterRingRadius);

               //Waves

               float waveCompMaskA = CalculateCompositeWaveMask(IN.worldToCenter, _WaveRingRadius.x, _WaveLineThickness.x, _WaveMoveInterval.x, _WaveAmpsA.x, _WaveFreqA.x, _WavePhaseA.x, _WaveAmpsA.y, _WaveFreqA.y, _WavePhaseA.y, _Time.y);
               float waveCompMaskB = CalculateCompositeWaveMask(IN.worldToCenter, _WaveRingRadius.y, _WaveLineThickness.y, _WaveMoveInterval.y, _WaveAmpsA.z, _WaveFreqA.z, _WavePhaseA.z, _WaveAmpsA.w, _WaveFreqA.w, _WavePhaseA.w, _Time.y);
               float waveCompMaskC = CalculateCompositeWaveMask(IN.worldToCenter, _WaveRingRadius.z, _WaveLineThickness.z, _WaveMoveInterval.z, _WaveAmpsB.x, _WaveFreqB.x, _WavePhaseB.x, _WaveAmpsB.y, _WaveFreqB.y, _WavePhaseB.y, _Time.y);
               float waveCompMaskD = CalculateCompositeWaveMask(IN.worldToCenter, _WaveRingRadius.w, _WaveLineThickness.w, _WaveMoveInterval.w, _WaveAmpsB.z, _WaveFreqB.z, _WavePhaseB.z, _WaveAmpsB.w, _WaveFreqB.w, _WavePhaseB.w, _Time.y);

               //Waves end

               float SpikeRingMask = MultiSpikeRingMask(IN.worldToCenter + _Center, _Center, _RingRadius - _RingWidth, _RingWidth);
               float hideRingMask = CalculateCircularMask(IN.worldToCenter, _RingRadius + _RingWidth * 2, .1);
               float SpikeVisible = SpikeRingMask;

               //float outerRingMask = CalculateRingMask(IN.worldToCenter, _OuterRingRadius, _OuterRingRadius + _OuterRingWidth, _OuterRingFeather, _OuterRingFeather, true);

               //Wave Colour Alpha modulation
               _WaveColourA *= _WaveAlphas.x;
               _WaveColourB *= _WaveAlphas.y;
               _WaveColourC *= _WaveAlphas.z;
               _WaveColourD *= _WaveAlphas.w;

               //Wave colour alpha end

               half4 ringBackgroundColour = lerp((0, 0, 0, 0), _BackGroundColour, ringBackgroundMask);
               half4 ringColour = lerp(ringBackgroundColour, _RingColour, ringMask);
               half4 ringWaveCompColourA = lerp(ringColour, _WaveColourA, waveCompMaskA * _WaveColourA.a);
               half4 ringWaveCompColourB = lerp(ringWaveCompColourA, _WaveColourB, waveCompMaskB * _WaveColourB.a);
               half4 ringWaveCompColourC = lerp(ringWaveCompColourB, _WaveColourC, waveCompMaskC * _WaveColourC.a);
               half4 ringWaveCompColourD = lerp(ringWaveCompColourC, _WaveColourD, waveCompMaskD * _WaveColourD.a);

               //half4 ringWaveFinalColour = lerp(ringWaveCompColourD, _RingColour, outerRingMask);
               half4 spikeCompositeColour = lerp(half4(0, 0, 0, 0), _SpikeMaskColour, SpikeVisible * (1 - hideRingMask));
               half4 finalColour = lerp(spikeCompositeColour, ringWaveCompColourD, 1 - spikeCompositeColour.a);

               return finalColour;
           }
           ENDHLSL
        }
    }
}
