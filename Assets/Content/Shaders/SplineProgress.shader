Shader "Custom/SplineProgress"
{
    Properties
    {
        _ActiveColor    ("Active Color",   Color)         = (1, 1, 1, 1)
        _InactiveColor  ("Inactive Color", Color)         = (1, 1, 1, 0.4)
        _Progress       ("Progress",       Range(0, 1))   = 0
        _EdgeSoftness   ("Edge Softness",  Range(0, 0.1)) = 0.02
        _UVMax          ("UV Max (auto-set by script)", Float) = 1.0
        _Reverse        ("Reverse",        Float)         = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float2 uv          : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ActiveColor;
                float4 _InactiveColor;
                float  _Progress;
                float  _EdgeSoftness;
                float  _UVMax;
                float  _Reverse;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv          = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float t = IN.uv.y / max(_UVMax, 0.0001);
                t = lerp(t, 1.0 - t, _Reverse);

                float blend = smoothstep(
                    _Progress - _EdgeSoftness,
                    _Progress + _EdgeSoftness,
                    t);

                half4 col = lerp(_ActiveColor, _InactiveColor, blend);

                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(IN.normalWS, mainLight.direction));
                col.rgb *= (0.5 + 0.5 * NdotL);

                return col;
            }
            ENDHLSL
        }
    }
}