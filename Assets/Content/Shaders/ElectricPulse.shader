Shader "IMVR/ElectricPulse"
{
    Properties
    {
        [HDR] _CoreColor        ("Core Color",          Color)              = (0.2, 0.6, 1.0, 1.0)
        [HDR] _RimColor         ("Rim / Arc Color",     Color)              = (0.5, 0.9, 1.0, 1.0)
        [HDR] _PulseColor       ("Pulse Ring Color",    Color)              = (1.0, 1.0, 1.0, 1.0)

        _RimPower               ("Rim Power",           Range(0.5, 8.0))    = 3.0
        _RimIntensity           ("Rim Intensity",       Range(0.0, 10.0))   = 1.5

        [Header(Pulse Rings)]
        _PulseCount             ("Ring Count",          Range(1.0, 8.0))    = 2.0
        _PulseSpeed             ("Pulse Speed",         Range(0.1, 5.0))    = 1.2
        _PulseWidth             ("Ring Band Width",     Range(0.001, 0.3))  = 0.04
        _PulseSharpness         ("Ring Sharpness",      Range(1.0, 60.0))   = 30.0

        [Header(Stacked Lines Per Ring)]
        _StackCount             ("Lines Per Ring",      Range(1.0, 6.0))    = 1.0
        _StackSpacing           ("Stack Spacing",       Range(0.002, 0.15)) = 0.025

        [Header(Ring Wobble)]
        _WobbleAmount           ("Wobble Amount",       Range(0.0, 0.4))    = 0.1
        _WobbleFrequency        ("Wobble Frequency",    Range(1.0, 20.0))   = 6.0
        _WobbleSpeed            ("Wobble Speed",        Range(0.0, 8.0))    = 2.0

        [Header(Electric Arcs)]
        _NoiseScale             ("Arc Noise Scale",     Range(1.0, 30.0))   = 8.0
        _NoiseSpeed             ("Arc Noise Speed",     Range(0.0, 5.0))    = 2.0
        _NoiseIntensity         ("Arc Intensity",       Range(0.0, 3.0))    = 0.8
        _NoiseThreshold         ("Arc Threshold",       Range(0.0, 1.0))    = 0.72

        [Header(Flicker)]
        _FlickerSpeed           ("Flicker Speed",       Range(0.0, 30.0))   = 12.0
        _FlickerAmount          ("Flicker Amount",      Range(0.0, 1.0))    = 0.15

        _GlobalIntensity        ("Global Intensity",    Range(0.0, 2.0))    = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
        }

        Blend One One
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ElectricPulse"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #pragma target   3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4  _CoreColor;
                half4  _RimColor;
                half4  _PulseColor;
                half   _RimPower;
                half   _RimIntensity;
                half   _PulseCount;
                half   _PulseSpeed;
                half   _PulseWidth;
                half   _PulseSharpness;
                half   _StackCount;
                half   _StackSpacing;
                half   _WobbleAmount;
                half   _WobbleFrequency;
                half   _WobbleSpeed;
                half   _NoiseScale;
                half   _NoiseSpeed;
                half   _NoiseIntensity;
                half   _NoiseThreshold;
                half   _FlickerSpeed;
                half   _FlickerAmount;
                half   _GlobalIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float2 uv         : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(Hash(i),                Hash(i + float2(1, 0)), u.x),
                    lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), u.x),
                    u.y);
            }

            float Fbm(float2 p)
            {
                return 0.60 * ValueNoise(p)
                     + 0.40 * ValueNoise(p * 2.1 + float2(5.3, 1.7));
            }

            Varyings Vert(Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varyings OUT;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs pos  = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   norm = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = pos.positionCS;
                OUT.positionWS = pos.positionWS;
                OUT.normalWS   = norm.normalWS;
                OUT.viewDirWS  = GetWorldSpaceViewDir(pos.positionWS);
                OUT.uv         = IN.uv;
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float  t        = _Time.y;
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDir  = normalize(IN.viewDirWS);

                float  NdotV    = saturate(dot(normalWS, viewDir));
                float  fresnel  = pow(1.0 - NdotV, _RimPower);
                half3  rimColor = _RimColor.rgb * fresnel * _RimIntensity;

                // atan2 maps horizontal position around the sphere to [0,1].
                // Used as the noise X-axis so each point on a ring gets
                // an independent up/down jag rather than the whole ring shifting together.
                float  angle    = atan2(normalWS.z, normalWS.x) / (2.0 * 3.14159265);

                // ── Ring + stack loop ──────────────────────────────────────
                int   ringCount  = (int)clamp(_PulseCount, 1.0, 8.0);
                int   stackCount = (int)clamp(_StackCount, 1.0, 6.0);
                float totalPulse = 0.0;

                for (int r = 0; r < ringCount; r++)
                {
                    // Spread rings evenly across the tiling period.
                    float ringPhaseOff = (float)r / (float)ringCount;

                    // Irrational-multiple seeds keep noise patterns from
                    // repeating between rings and stacks.
                    float rSeed = (float)r * 3.733;

                    for (int s = 0; s < stackCount; s++)
                    {
                        float sSeed    = (float)s * 1.913;
                        float stackOff = ((float)s - (float)(stackCount - 1) * 0.5)
                                       * _StackSpacing;

                        float wobbleUV = angle * _WobbleFrequency
                                       + t     * _WobbleSpeed
                                       + rSeed + sSeed;
                        float wobble   = (ValueNoise(float2(wobbleUV,
                                                            t * 0.31 + rSeed + sSeed))
                                         - 0.5) * 2.0 * _WobbleAmount;

                        float perturbedY = normalWS.y + wobble + stackOff;

                        // Map [-1,1] normalWS.y to [0,1], subtract travel and ring offset.
                        float tiled = perturbedY * 0.5 + 0.5
                                    - t          * _PulseSpeed
                                    - ringPhaseOff;

                        float phase = frac(tiled);
                        float dist  = abs(phase - 0.5);
                        float band  = saturate(1.0 - dist / max(_PulseWidth * 0.5, 0.001));
                        band        = pow(band, _PulseSharpness);

                        totalPulse += band;
                    }
                }

                // saturate so overlapping stacks don't blow out the colour.
                half3 pulseColor = _PulseColor.rgb * saturate(totalPulse);

                // ── Electric arc noise ─────────────────────────────────────
                float2 noiseUV  = IN.uv * _NoiseScale
                                + float2(t * _NoiseSpeed, t * _NoiseSpeed * 0.7);
                float  arc      = smoothstep(_NoiseThreshold, 1.0, Fbm(noiseUV));
                half3  arcColor = _RimColor.rgb * arc * fresnel * _NoiseIntensity;

                half3  core     = _CoreColor.rgb * (1.0 - NdotV) * 0.3;

                float  flicker  = 1.0 + _FlickerAmount * (
                                      sin(t * _FlickerSpeed * 1.0) *
                                      sin(t * _FlickerSpeed * 2.3) *
                                      sin(t * _FlickerSpeed * 0.7));

                return half4((rimColor + pulseColor + arcColor + core) * flicker * _GlobalIntensity, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
