// Shader "Unlit/WaveShader"
// {
//     Properties
//     {
//         _MainTex ("Texture", 2D) = "white" {}
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Opaque" }
//         LOD 100

//         Pass
//         {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             // make fog work
//             #pragma multi_compile_fog

//             #include "UnityCG.cginc"

//             struct appdata
//             {
//                 float4 vertex : POSITION;
//                 float2 uv : TEXCOORD0;
//             };

//             struct v2f
//             {
//                 float2 uv : TEXCOORD0;
//                 UNITY_FOG_COORDS(1)
//                 float4 vertex : SV_POSITION;
//             };

//             sampler2D _MainTex;
//             float4 _MainTex_ST;

//             v2f vert (appdata v)
//             {
//                 v2f o;
//                 o.vertex = UnityObjectToClipPos(v.vertex);
//                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                 UNITY_TRANSFER_FOG(o,o.vertex);
//                 return o;
//             }

//             fixed4 frag (v2f i) : SV_Target
//             {
//                 // sample the texture
//                 fixed4 col = tex2D(_MainTex, i.uv);
//                 // apply fog
//                 UNITY_APPLY_FOG(i.fogCoord, col);
//                 return col;
//             }
//             ENDCG
//         }
//     }
// }

//UNITY_SHADER_NO_UPGRADE

Shader "Unlit/WaveShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Period ("Period", Float) = 1

		_Amplitude ("Amplitude", Float) = 0.25
        // _Time ("Time", Time) = T
	}
	SubShader
	{
		Pass
		{
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
            float4 _MainTex_ST;

            uniform float4x4 _CustomMVP;



			float _Period;
			float _Amplitude;

			struct MeshData
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Interpolators
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// UNITY_MATRIX_MP = ;

			// Vertex Shader
			// Implementation of the vertex shader
			Interpolators vert(MeshData v)
			{
				// Interpolators o;
                // o.vertex = UnityObjectToClipPos(v.vertex);
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);



				// Displace the original vertex in model space
				// float4 displacement = float4(0.0f, _Amplitude * sin( (v.vertex.x + _Period * _Time.y  )), 0.0f, 0.0f);
				// v.vertex += displacement;
				// v.vertex = mul(UNITY_MATRIX_M, v.vertex);

				// o.vertex = mul(UNITY_MATRIX_VP, v.vertex);

                // o.vertex =  mul(_CustomMVP, v.vertex); 
				// o.uv = v.uv;
				// return o;

				_Amplitude = 0.25;


				Interpolators o;
				// Displace the original vertex in model space
				v.vertex = mul(UNITY_MATRIX_M, v.vertex);
				float4 displacement = float4(0.0f, _Amplitude * sin( (v.vertex.x + _Period * _Time.y  )), 0.0f, 0.0f);
				v.vertex += displacement;

				o.vertex = mul(UNITY_MATRIX_VP, v.vertex);
				o.uv = v.uv;
                o.uv.x += _Time.y * 0.1;

				return o;
			}
			
			// Fragment Shader
			// Implementation of the fragment shader
			fixed4 frag(Interpolators v) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, v.uv);
				return col;
			}
			ENDCG
		}
	}
}