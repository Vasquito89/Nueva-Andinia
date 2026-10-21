Shader "Custom/VHS_UIToolkit"
{
    Properties
    {
        [HideInInspector] _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _EffectIntensity ("Intensidad del Ruido", Range(0, 1)) = 0.5

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_VHS_Overlay_White"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            float4 _Color;
            float _EffectIntensity;

            #define V float2(0.0, 1.0)
            #define VHSRES float2(320.0, 240.0)
            #define saturateVal(i) clamp(i, 0.0, 1.0)

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float v2random(float2 uv)
            {
                return hash21(uv);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float time = _Time.y;

                float noiseAlpha = 0.0;

                // Ruido aleatorio puntual (glitch horizontal fino)
                float tcNoise = smoothstep(0.4, 1.0, v2random(float2(uv.y * 4.77, time)));
                if (tcNoise > 0.5)
                {
                    float2 uvt = (uv + V.yx * v2random(float2(uv.y, time))) * float2(0.1, 1.0);
                    float n0 = v2random(uvt);
                    float n1 = v2random(uvt + V.yx / VHSRES.x);
                    if (n1 < n0)
                    {
                        noiseAlpha += pow(n0, 6.0) * 0.7;
                    }
                }

                // Scanlines continuas finas
                float scanline = sin(uv.y * 400.0 + time * 10.0) * 0.04;
                
                // Grano fino sutil
                float grain = (hash21(uv + time) - 0.5) * 0.10;
                
                // Solo acumulamos grano y scanlines (cero franjas anchas)
                noiseAlpha += scanline + grain;
                noiseAlpha = saturateVal(noiseAlpha * _EffectIntensity);

                float3 finalColor = float3(1.0, 1.0, 1.0);
                return float4(finalColor, noiseAlpha * input.color.a);
            }
            ENDHLSL
        }
    }
}