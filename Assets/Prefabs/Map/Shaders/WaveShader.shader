//UNITY_SHADER_NO_UPGRADE

Shader "Unlit/WaveShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Period ("Period", Float) = 1
		_Amplitude ("Amplitude", Float) = 1
		_Gloss("Gloss", Float) = 1
		_fAtt("fAtt", Float) = 1
		_Kd("Kd", Float) = 1
		_Ka("Ka", Float) = 1
	}
	SubShader
	{
		Pass
		{
			Cull Off

			Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

			#include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

			uniform sampler2D _MainTex;
            float4 _MainTex_ST;

			float _Gloss;
			float _fAtt;
			float _Kd;
			float _Ka;

			float _Period;
			float _Amplitude;

			struct MeshData {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct Interpolators {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float wave : TEXCOORD1;
				float3 normal : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				SHADOW_COORDS(4) // put shadows data into TEXCOORD4
			};

			// Vertex Shader
			// Implementation of the vertex shader
			Interpolators vert(MeshData v) {
				Interpolators o;
				// Displace the original vertex in model space
				o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
				v.vertex = mul(UNITY_MATRIX_M, v.vertex);

				// Displace the vertex by the wave
				float4 displacement = float4(0.0f, _Amplitude * sin( (v.vertex.x + _Period * _Time.y )), 0.0f, 0.0f);
				v.vertex += displacement;
				o.wave = displacement.y;

				o.pos = mul(UNITY_MATRIX_VP, v.vertex);
				o.uv = v.uv;

				// 	Tile the texture by time
                o.uv.x -= _Time.y * 0.1;
				o.normal = UnityObjectToWorldNormal( v.normal);
				TRANSFER_SHADOW(o)
				return o;
			}
			
			// Fragment Shader
			// Implementation of the fragment shader
			fixed4 frag(Interpolators i) : SV_Target {
				fixed4 unlitCol = tex2D(_MainTex, i.uv);

				// Calculate ambient RGB intensities
				float3 amb = unlitCol.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * _Ka;
				float4 ambientLightF4 = float4(amb,1);
				
				// Phong Illumination -> Lambertian diffusion
                float3 N = i.normal;
                float3 L = _WorldSpaceLightPos0.xyz; // Light Direction from the world light
                float3 lambertianDiffuse = saturate(dot(N,L)); // L dot N clamped between 0 and 1
                float3 diffuseLight = lambertianDiffuse * _LightColor0.xyz * _fAtt * _Kd;
                float4 diffuseLightF4 = float4(diffuseLight.xxx, 1);
                
                // Specular light through Blinn Phong
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 HalfVector = normalize(L +V);
                float3 specLight = saturate(dot(HalfVector, N)) * (lambertianDiffuse > 0);
                specLight = pow(specLight, _Gloss);
                float4 specLightF4 = float4(specLight.xxx, 1);
				
				fixed shadow = SHADOW_ATTENUATION(i);
				return unlitCol * diffuseLightF4 *(shadow+0.5) + i.wave*shadow + specLightF4*shadow +ambientLightF4;
			}
			ENDCG
		}
	}
}