// // Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/FogShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,0.5,0.5,1)
        _BgColor ("Background Color", Color) = (1,0.5,0.5,1)
        _MaxDist("Maximum Distance", Float) = 100
        _MinDist("Minimum Distance", Float) = 20
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            // Fog {}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 cameraDistance : TEXCOORD1;
                float4 worldDistance : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MaxDist;
            float _MinDist;
            float4 _Color;
            float4 _BgColor;

            float getFogFactor(float d) {
                if (d<=_MinDist) return 0;
                if (d>=_MaxDist) return 1;

                return 1 - (_MaxDist - d) / (_MaxDist - _MinDist);
            }

            float4 mix(float4 x, float4 y, float a) {
                return x*(1-a)+y*a;
            }

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.cameraDistance = UnityObjectToViewPos(v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
                
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                // sample the texture
                float fogAmount = getFogFactor(-i.cameraDistance.z);

                fixed4 blend = mix(_Color, _BgColor, fogAmount);
                return blend;
            }
            ENDCG
        }
    }
}