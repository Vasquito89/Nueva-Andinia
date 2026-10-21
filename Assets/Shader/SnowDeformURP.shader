Shader "Custom/SnowDeformURP"
{
    // Popiedades que se pueden ajustar en el editor de materiales
    // Cuenta con dos texturas la de la nieve y la del suelo
    // un mascara q segun su color hace ver nieve o suelo
    Properties
    {
        _SnowTex   ("Snow Texture", 2D) = "white" {}
        _GroundTex ("Ground / Pressed Texture", 2D) = "white" {}
        _MaskTex   ("Mask (Black=Snow, White=Ground)", 2D) = "white" {}

        _SnowTiling   ("Snow Tiling", Vector) = (4,4,0,0)
        _GroundTiling ("Ground Tiling", Vector) = (4,4,0,0)

        _SnowHeight ("Snow Height", Float) = 0.08
        // Dirección en espacio objeto en la que se desplaza la nieve
        // este se puso por si el objeto lleva su arriba en otra direccion
        _DisplaceDir ("Displacement Direction (Object Space)", Vector) = (0,1,0,0)

        _IndirectStrength ("Indirect Strength", Range(0,1)) = 0.2
    }

    SubShader
    {
        // Es un material opaco
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

            // Soporte instancing y niebla
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            // Las luces por alguna razon no funcionaban si no tenia esto
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            // Inclusion de librerias de URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Definicion de texturas y constantes
            TEXTURE2D(_SnowTex);    SAMPLER(sampler_SnowTex);
            TEXTURE2D(_GroundTex);  SAMPLER(sampler_GroundTex);
            TEXTURE2D(_MaskTex);    SAMPLER(sampler_MaskTex);

            // Meter constantes en cbuffer para mejor rendimiento
            CBUFFER_START(UnityPerMaterial)
                float4 _SnowTiling;
                float4 _GroundTiling;
                float  _SnowHeight;
                float4 _DisplaceDir;
                float  _IndirectStrength;
            CBUFFER_END

            // Lo que entra al vertex shader desde la mesh
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Lo que sale del vertex shader y entra al fragment shader
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3  normalWS   : TEXCOORD1;
                float2 uvSnow     : TEXCOORD2;
                float2 uvGround   : TEXCOORD3;
                half   snowAmt    : TEXCOORD4;
                half   fogFactor  : TEXCOORD5;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Aqui se hace el desplazamiento de la nieve
            Varyings vert (Attributes v)
            {
                // Inicializaciones necesarias
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float2 uv0 = v.uv;

                // Leer la mascara y convertirla en cantidad de nieve
                float maskR = SAMPLE_TEXTURE2D_LOD(_MaskTex, sampler_MaskTex, uv0, 0).r;
                float snowAmt = saturate(1.0 - maskR);

                // Desplazar la posicion de los vertices segun la cantidad de nieve
                float3 dirOS = normalize(_DisplaceDir.xyz);
                float3 posOS = v.positionOS.xyz + dirOS * (snowAmt * _SnowHeight);

                // Calculos de posiciones y normales
                VertexPositionInputs posInputs = GetVertexPositionInputs(posOS);
                VertexNormalInputs   nInputs   = GetVertexNormalInputs(v.normalOS);

                
                o.positionCS = posInputs.positionCS;
                o.positionWS = posInputs.positionWS;
                o.normalWS   = normalize(nInputs.normalWS);

                // UVs y fog
                o.uvSnow   = uv0 * _SnowTiling.xy;
                o.uvGround = uv0 * _GroundTiling.xy;
                o.snowAmt  = (half)snowAmt;

                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            // Esto es para evitar que la luz principal y las adicionales no iluminen por detras
            // o que se quede todo negro
            inline half NdotL_Safe(half3 n, half3 lightDirFromURP)
            {
                // Sin abs: no ilumina por detrás. Solo escogemos el signo correcto.
                half3 La = normalize(lightDirFromURP);
                half3 Lb = -La;
                return max(saturate(dot(n, La)), saturate(dot(n, Lb)));
            }

            // Aqui se leen las texturas y se hace la mezcla 
            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Leer texturas y mezclar segun la mascara
                half3 snowCol   = SAMPLE_TEXTURE2D(_SnowTex, sampler_SnowTex, i.uvSnow).rgb;
                half3 groundCol = SAMPLE_TEXTURE2D(_GroundTex, sampler_GroundTex, i.uvGround).rgb;
                half3 albedo    = lerp(groundCol, snowCol, i.snowAmt);

                half3 n = normalize(i.normalWS);

                // Indirect suave (probes)
                half3 col = albedo * (SampleSH(n) * (half)_IndirectStrength);

                // Main light (sin atten/shadows para que no muera en Play)
                Light mainLight = GetMainLight();
                half ndlMain = NdotL_Safe(n, (half3)mainLight.direction);
                col += albedo * (half3)mainLight.color * ndlMain;

                // Additional lights: soporta _ADDITIONAL_LIGHTS y _ADDITIONAL_LIGHTS_VERTEX
                #if defined(_ADDITIONAL_LIGHTS) || defined(_ADDITIONAL_LIGHTS_VERTEX)
                uint count = GetAdditionalLightsCount();
                for (uint li = 0u; li < count; li++)
                {
                    Light light = GetAdditionalLight(li, i.positionWS);
                    half ndl = NdotL_Safe(n, (half3)light.direction);
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
