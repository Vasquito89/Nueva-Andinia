Shader "Custom/SnowBlendURP"
{
    Properties
    {
        [Header(BaseGround)]
        _BaseAlbedoTex ("Base Albedo", 2D) = "white" {}
        [Normal]_BaseNormalTex ("Base Normal", 2D) = "bump" {}
        _BaseTiling    ("Base Tiling (XY)", Vector) = (1,1,0,0)

        [Header(Snow)]
        _SnowAlbedoTex ("Snow Albedo", 2D) = "white" {}
        [Normal]_SnowNormalTex ("Snow Normal", 2D) = "bump" {}
        _SnowTriplanarScale ("Snow Triplanar Scale", Float) = 1
        _TriplanarSharpness ("Triplanar Sharpness", Range(1,16)) = 6

        [Header(DynamicMask)]
        _SnowDynamicMaskTex ("Snow Dynamic Mask (R)", 2D) = "white" {}
        _WorldSize          ("World Size (for mask mapping)", Float) = 16

        [Header(SlopeParams)]
        _SnowAmount     ("Snow Amount", Range(0,2)) = 1
        _SnowSoftness   ("Snow Softness (pow)", Float) = 2
        _SlopeThreshold ("Slope Threshold", Range(0,1)) = 0.7
        _SlopeFade      ("Slope Fade", Range(0.001,0.5)) = 0.15

        // Tu “arriba” custom (WORLD)
        _SnowUp ("Snow Up Direction (World)", Vector) = (0,1,0,0)

        [Header(Normals)]
        _NormalStrength     ("Normal Strength", Range(0,2)) = 1
        _NormalAffectsSnow  ("Normal Affects Snow", Range(0,1)) = 0.6

        [Header(EdgeBreakup)]
        _EdgeNoiseTex ("Edge Noise (R)", 2D) = "gray" {}
        _EdgeNoiseScale ("Edge Noise Scale", Float) = 0.5
        _EdgeNoiseStrength ("Edge Noise Strength", Range(0,0.3)) = 0.08

        [Header(Lighting)]
        _IndirectStrength ("Indirect Strength", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" "RenderType"="Opaque" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            // Instanciacion y niebla
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            // Luces
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Declaracion de texturas
            TEXTURE2D(_BaseAlbedoTex);        SAMPLER(sampler_BaseAlbedoTex);
            TEXTURE2D(_BaseNormalTex);        SAMPLER(sampler_BaseNormalTex);

            TEXTURE2D(_SnowAlbedoTex);        SAMPLER(sampler_SnowAlbedoTex);
            TEXTURE2D(_SnowNormalTex);        SAMPLER(sampler_SnowNormalTex);

            TEXTURE2D(_SnowDynamicMaskTex);   SAMPLER(sampler_SnowDynamicMaskTex);

            TEXTURE2D(_EdgeNoiseTex);         SAMPLER(sampler_EdgeNoiseTex);

            // Buffer de constantes
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseTiling;

                float  _SnowTriplanarScale;
                float  _TriplanarSharpness;

                float  _WorldSize;

                float  _SnowAmount;
                float  _SnowSoftness;
                float  _SlopeThreshold;
                float  _SlopeFade;

                float4 _SnowUp;

                float  _NormalStrength;
                float  _NormalAffectsSnow;

                float  _EdgeNoiseScale;
                float  _EdgeNoiseStrength;

                float  _IndirectStrength;
            CBUFFER_END

            // Declaracion de estructuras
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;

                half3  normalWS   : TEXCOORD1;
                half3  tangentWS  : TEXCOORD2;
                half   tangentSign: TEXCOORD3;

                float2 uvBase     : TEXCOORD4;
                half   fogFactor  : TEXCOORD5;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Esta funcion devuelve una matriz TBN (tangente, bitangente, normal)
            // en mundo
            float3x3 BuildTBNSafe(float3 nWS, float3 tWS, float sign)
            {
                // Normaliza normal, comprueba si la tangente es casi cero
                // en ese caso, normaliza la tangente 
                // Para poder pasar un normal map de tangent space a world space
                // aunque el mesh no tenga tangentes bien definidas
                float3 n = normalize(nWS);
                float3 t = tWS;
                float tLen2 = dot(t, t);

                if (tLen2 < 1e-6)
                {
                    float3 up = (abs(n.y) < 0.999) ? float3(0,1,0) : float3(1,0,0);
                    t = normalize(cross(up, n));
                    float3 b = normalize(cross(n, t));
                    return float3x3(t, b, n);
                }
                else
                {
                    t = normalize(t);
                    float3 b = normalize(cross(n, t) * sign);
                    return float3x3(t, b, n);
                }
            }

            // Controla la fuerza del normal map
            half3 ApplyNormalStrength(half3 nTS, half strength)
            {
                nTS.xy *= strength;
                nTS.z = sqrt(saturate(1.0h - dot(nTS.xy, nTS.xy)));
                return normalize(nTS);
            }

            // ---- Triplanar helpers ----
            float3 TriplanarWeights(float3 n, float sharpness)
            {
                float3 an = abs(n);
                an = pow(an, sharpness);
                return an / max(1e-6, (an.x + an.y + an.z));
            }

            half3 SampleTriplanarAlbedo(Texture2D tex, SamplerState samp, float3 posWS, float3 nWS, float scale, float sharpness)
            {
                float3 w = TriplanarWeights(nWS, sharpness);

                float2 uvX = posWS.zy * scale; // projection on YZ (X axis)
                float2 uvY = posWS.xz * scale; // projection on XZ (Y axis)
                float2 uvZ = posWS.xy * scale; // projection on XY (Z axis)

                half3 cx = SAMPLE_TEXTURE2D(tex, samp, uvX).rgb;
                half3 cy = SAMPLE_TEXTURE2D(tex, samp, uvY).rgb;
                half3 cz = SAMPLE_TEXTURE2D(tex, samp, uvZ).rgb;

                return cx * (half)w.x + cy * (half)w.y + cz * (half)w.z;
            }

            // Esto es para tener el normal mapping triplanar en world space
            // para poder mezclar con el normal map base
            half3 SampleTriplanarNormalWS(Texture2D tex, SamplerState samp, float3 posWS, float3 nWS, float scale, float sharpness, half normalStrength)
            {
                // Pesos de cada proyección y signo
                float3 w = TriplanarWeights(nWS, sharpness);

                float sx = (nWS.x >= 0.0) ? 1.0 : -1.0;
                float sy = (nWS.y >= 0.0) ? 1.0 : -1.0;
                float sz = (nWS.z >= 0.0) ? 1.0 : -1.0;

                // UVs y sampleo de normal maps
                float2 uvX = posWS.zy * scale;
                float2 uvY = posWS.xz * scale;
                float2 uvZ = posWS.xy * scale;

                half3 nxTS = ApplyNormalStrength(UnpackNormal(SAMPLE_TEXTURE2D(tex, samp, uvX)), normalStrength);
                half3 nyTS = ApplyNormalStrength(UnpackNormal(SAMPLE_TEXTURE2D(tex, samp, uvY)), normalStrength);
                half3 nzTS = ApplyNormalStrength(UnpackNormal(SAMPLE_TEXTURE2D(tex, samp, uvZ)), normalStrength);

                // Bases por proyeccion:
                // X projection (onto YZ): tangent=+Z, bitangent=+Y, normal=+X
                float3 Tx = float3(0,0,1) * sx;
                float3 Bx = float3(0,1,0);
                float3 Nx = float3(1,0,0) * sx;
                half3 nX = normalize((half3)(Nx * nxTS.z + Tx * nxTS.x + Bx * nxTS.y));

                // Y projection (onto XZ): tangent=+X, bitangent=+Z, normal=+Y
                float3 Ty = float3(1,0,0);
                float3 By = float3(0,0,1) * sy;
                float3 Ny = float3(0,1,0) * sy;
                half3 nY = normalize((half3)(Ny * nyTS.z + Ty * nyTS.x + By * nyTS.y));

                // Z projection (onto XY): tangent=+X, bitangent=+Y, normal=+Z
                float3 Tz = float3(1,0,0);
                float3 Bz = float3(0,1,0) * sz;
                float3 Nz = float3(0,0,1) * sz;
                half3 nZ = normalize((half3)(Nz * nzTS.z + Tz * nzTS.x + Bz * nzTS.y));

                half3 nBlend = normalize(nX * (half)w.x + nY * (half)w.y + nZ * (half)w.z);
                return nBlend;
            }

            Varyings vert (Attributes v)
            {
                // Setup
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs pos = GetVertexPositionInputs(v.positionOS.xyz); // Saca clip/world pos
                VertexNormalInputs   nor = GetVertexNormalInputs(v.normalOS, v.tangentOS); // Saca normal/tangente mundo

                // Se guarda
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;

                o.normalWS = normalize(nor.normalWS);

                // Convierte tangente a world space
                float3 tWS = TransformObjectToWorldDir(v.tangentOS.xyz);
                o.tangentWS = normalize((half3)tWS);
                o.tangentSign = (abs(v.tangentOS.w) < 1e-6) ? 1.0h : (half)v.tangentOS.w;

                // Calcula UVs base
                o.uvBase = v.uv * _BaseTiling.xy + _BaseTiling.zw;

                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            // Evita que se quede negro si URP da el signo mal (lo de la iluminacion)
            inline half NdotL_Safe(half3 n, half3 lightDirFromURP)
            {
                half3 La = normalize(lightDirFromURP);
                half3 Lb = -La;
                return max(saturate(dot(n, La)), saturate(dot(n, Lb)));
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Setup
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float3 posWS = i.positionWS;
                half3  nGeomWS = normalize(i.normalWS);

                half3 snowUp = normalize((half3)_SnowUp.xyz);

                // Material base para el normal map (Base UV)
                half3 baseCol = SAMPLE_TEXTURE2D(_BaseAlbedoTex, sampler_BaseAlbedoTex, i.uvBase).rgb;

                float3x3 TBN = BuildTBNSafe(nGeomWS, i.tangentWS, (float)i.tangentSign);
                half3 baseN_TS = ApplyNormalStrength(UnpackNormal(SAMPLE_TEXTURE2D(_BaseNormalTex, sampler_BaseNormalTex, i.uvBase)), (half)_NormalStrength);
                half3 baseN_WS = normalize((half3)mul(TBN, (float3)baseN_TS));

                // Snow (TRIPLANAR), nieve en world triplanar para que no estire en los lados
                float snowScale = max(1e-5, _SnowTriplanarScale);
                half3 snowCol = SampleTriplanarAlbedo(_SnowAlbedoTex, sampler_SnowAlbedoTex, posWS, nGeomWS, snowScale, _TriplanarSharpness);
                half3 snowN_WS = SampleTriplanarNormalWS(_SnowNormalTex, sampler_SnowNormalTex, posWS, nGeomWS, snowScale, _TriplanarSharpness, (half)_NormalStrength);

                // Mascara world en XZ que define donde hay nieve
                float2 dynUV = (posWS.xz + _WorldSize) / (2.0 * _WorldSize);
                half dynMask = SAMPLE_TEXTURE2D(_SnowDynamicMaskTex, sampler_SnowDynamicMaskTex, dynUV).r;

                // Offset pequenno para que el borde no sea perfecto
                half edgeNoise = SAMPLE_TEXTURE2D(_EdgeNoiseTex, sampler_EdgeNoiseTex, posWS.xz * _EdgeNoiseScale).r;
                edgeNoise = (edgeNoise - 0.5h) * 2.0h; // -1..1
                half noiseOffset = edgeNoise * (half)_EdgeNoiseStrength;

                // Slope macro: desde arriba (NO nieve abajo)
                // Mascara grande por pendiente/orientacion
                half geomUp = dot(nGeomWS, snowUp);
                geomUp = saturate(geomUp); // corta todo lo que este “mirando abajo”

                geomUp = saturate(geomUp * (half)_SnowAmount + noiseOffset);

                half macroMask = smoothstep((half)_SlopeThreshold, (half)(_SlopeThreshold + _SlopeFade), geomUp);
                macroMask = pow(macroMask, max((half)0.0001, (half)_SnowSoftness));

                // Micro mask con normal map (para que “entre” la nieve en laterales donde el normalmap apunte hacia arriba)
                // Normal map base que ayuda a que haya nieve en micro-relieves
                half microUp = saturate(dot(baseN_WS, snowUp));
                half microMask = microUp;
                half slopeMask = macroMask * lerp(1.0h, microMask, (half)_NormalAffectsSnow); // Combiancion

                half snowMask = slopeMask * dynMask; // Mascara final para la nieve

                // Normal final para iluminación (mezcla base y snow)
                half3 nWS = normalize(lerp(baseN_WS, snowN_WS, snowMask));

                // Albedo final
                half3 albedo = lerp(baseCol, snowCol, snowMask);

                // Lambert y niebla: Luz
                half3 col = albedo * (SampleSH(nWS) * (half)_IndirectStrength);

                Light mainLight = GetMainLight();
                half ndlMain = NdotL_Safe(nWS, (half3)mainLight.direction);
                col += albedo * (half3)mainLight.color * ndlMain;

                #if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
                uint count = GetAdditionalLightsCount();
                for (uint li = 0u; li < count; li++)
                {
                    Light light = GetAdditionalLight(li, posWS);
                    half ndl = NdotL_Safe(nWS, (half3)light.direction);
                    col += albedo * (half3)light.color * ndl;
                }
                #endif

                col = MixFog(col, i.fogFactor);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
