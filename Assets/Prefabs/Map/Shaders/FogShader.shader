// // Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/FogShader"
{
    Properties
    {
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
                float4 vertex : SV_POSITION;
                float3 cameraDistance : TEXCOORD1;
                float4 worldDistance : TEXCOORD2;
            };

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
                o.worldDistance = mul(unity_ObjectToWorld, v.vertex);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
                
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                // Include Fog
                float fogAmount = getFogFactor(-i.cameraDistance.z);
                fixed4 blend = mix(_Color, _BgColor, fogAmount);

                // Add Watercolour pattern. Mulitiplied by 0.04 to increase size of pattern
                fixed3 waterColorVertex = (i.worldDistance)*0.04;

                // For loop for stilness of watercolour pattern
                for(int n = 1; n<2; n++) {
                    float i = float(n);
                    waterColorVertex = waterColorVertex + float3(0.5/i*sin(i*waterColorVertex.y + _Time.y + 0.3*i) + 0.8, 0.5/i*sin(waterColorVertex.z + _Time.y + 0.3*i) + 0.8, 0.5/i*sin(waterColorVertex.x + _Time.y + 0.3*i) + 0.8);
                }

                // black and white water pattern on all three axis
                fixed4 col = float4(sin(waterColorVertex.x + waterColorVertex.y + waterColorVertex.z),sin(waterColorVertex.x + waterColorVertex.y + waterColorVertex.z),sin(waterColorVertex.x + waterColorVertex.y + waterColorVertex.z), sin(waterColorVertex.x + waterColorVertex.y + waterColorVertex.z));

                
                return blend+(col/40);
            }
            ENDCG
        }
    }
}