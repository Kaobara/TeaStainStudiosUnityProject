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









// //     SubShader {
// //         Pass {
// //             CGPROGRAM
// //             #pragma vertex vert
// //             #pragma fragment frag
            
// //             //Needed for fog variation to be compiled.
// //             #pragma multi_compile_fog /

// //             #include "UnityCG.cginc"

// //             struct vertexInput {
// //                 float4 vertex : POSITION;
// //                 float4 texcoord0 : TEXCOORD0;
// //             };

// //             struct fragmentInput{
// //                 float4 position : SV_POSITION;
// //                 float4 texcoord0 : TEXCOORD0;
                
// //                 //Used to pass fog amount around number should be a free texcoord.
// //                 UNITY_FOG_COORDS(1)
// //             };

// //             fragmentInput vert(vertexInput i){
// //                 fragmentInput o;
// //                 o.position = UnityObjectToClipPos (i.vertex);
// //                 o.texcoord0 = i.texcoord0;
                
// //                 //Compute fog amount from clip space position.
// //                 UNITY_TRANSFER_FOG(o,o.position);
// //                 return o;
// //             }

// //             fixed4 frag(fragmentInput i) : SV_Target {
// //                 fixed4 color = fixed4(i.texcoord0.xy,0,0);
                
// //                 //Apply fog (additive pass are automatically handled)
// //                 UNITY_APPLY_FOG(i.fogCoord, color); 
                
// //                 //to handle custom fog color another option would have been 
// //                 //#ifdef UNITY_PASS_FORWARDADD
// //                 //  UNITY_APPLY_FOG_COLOR(i.fogCoord, color, float4(0,0,0,0));
// //                 //#else
// //                 //  fixed4 myCustomColor = fixed4(0,0,1,0);
// //                 //  UNITY_APPLY_FOG_COLOR(i.fogCoord, color, myCustomColor);
// //                 //#endif
                
// //                 return color;
// //             }
// //             ENDCG
// //         }
// //     }
// // }

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader "Unlit/FogShader"
// {
//     Properties
//     {
//         _MainTex ("Texture", 2D) = "white" {}
//         _Color ("Main Color", Color) = (1,0.5,0.5,1)
//         _BgColor ("Background Color", Color) = (1,0.5,0.5,1)
//         _MaxDist("Maximum Distance", Float) = 100
//         _MinDist("Minimum Distance", Float) = 20
//         _Gloss("Gloss", Float) = 1
        
//     }
//     SubShader
//     {
//         Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
//         // ags {}
//         LOD 100

//         Pass
//         {
//             // Fog {}

//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             // make fog work
//             #pragma multi_compile_fog
// #include "Lighting.cginc"
//             #include "AutoLight.cginc"

//             #include "UnityCG.cginc"

//             struct MeshData
//             {
//                 float4 vertex : POSITION;
//                 float2 uv : TEXCOORD0;
//                 float3 normal : NORMAL;
//                 float4 color : COLOR;
//             };

//             struct Interpolators
//             {
//                 float2 uv : TEXCOORD0;
//                 UNITY_FOG_COORDS(1)
//                 float4 vertex : SV_POSITION;
//                 float3 cameraDistance : TEXCOORD1;
//                 float4 worldDistance : TEXCOORD2;
//                 float4 color : COLOR;
//             };

//             sampler2D _MainTex;
//             float4 _MainTex_ST;
//             float _MaxDist;
//             float _MinDist;
//             float4 _Color;
//             float4 _BgColor;

//             float _Gloss;

//             float getFogFactor(float d) {
//                 // const float FogMax = 20.0;
//                 // const float FogMin = 10.0;

//                 if (d<=_MinDist) return 0;
//                 if (d>=_MaxDist) return 1;
//                 // if (d<=FogMin) return 0;
//                 // if (d>=FogMax) return 1;

//                 // return 1 - (FogMax - d) / (FogMax - FogMin);
//                 return 1 - (_MaxDist - d) / (_MaxDist - _MinDist);
//             }

//             float4 mix(float4 x, float4 y, float a) {
//                 // return x + (y-x)*a;

//                 return x*(1-a)+y*a;
//             }

//             Interpolators vert (MeshData v)
//             {
//                 Interpolators o;
                









//                 // Convert Vertex position and corresponding normal into world coords.
// 				// Note that we have to multiply the normal by the transposed inverse of the world 
// 				// transformation matrix (for cases where we have non-uniform scaling; we also don't
// 				// care about the "fourth" dimension, because translations don't affect the normal) 
// 				float4 worldVertex = mul(unity_ObjectToWorld, v.vertex);
// 				float3 worldNormal = normalize(mul(transpose((float3x3)unity_WorldToObject), UnityObjectToWorldNormal( v.normal).xyz));

// 				// Calculate diffuse RBG reflections, we save the results of L.N because we will use it again

//                 float3 N = UnityObjectToWorldNormal( v.normal);
//                 float3 L = _WorldSpaceLightPos0.xyz; // Light Direction
//                 float3 lambertianDiffuse = saturate(dot(N,L));
//                 float3 dif = lambertianDiffuse * _LightColor0.xyz;
//                 // float4 diffuseLightF4 = float4(diffuseLight.xxx, 1);
				
// 				// Calculate specular reflections
// 				// float Ks = _Ks;
// 				// float specN = _specN; // Values>>1 give tighter highlights
// 				float3 V = normalize(_WorldSpaceCameraPos - worldVertex.xyz);
//                 float3 HalfVector = normalize(L +V);
//                 float3 specLight = saturate(dot(HalfVector, N)) * (lambertianDiffuse > 0);
//                 specLight = pow(specLight, _Gloss);
//                 // float4 specLightF4 = float4(specLight.xxx, 1);
				
// 				// Combine Phong illumination model components
// 				o.color.rgb = dif.rgb + specLight.rgb;
// 				o.color.a = v.color.a;

// 				// Transform vertex in world coordinates to camera coordinates
// 				o.vertex = UnityObjectToClipPos(v.vertex);

//                 o.cameraDistance = UnityObjectToViewPos(v.vertex);
//                 // o.vertex = UnityObjectToClipPos(v.vertex);
//                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);

//                 UNITY_TRANSFER_FOG(o,o.vertex);
//                 return o;
//             }

//             fixed4 frag (Interpolators i) : SV_Target
//             {
//                 // sample the texture
//                 fixed4 col = tex2D(_MainTex, i.uv);
//                 // apply fog
//                 UNITY_APPLY_FOG(i.fogCoord, col);

//                 float pct = abs(sin(_Time.y));

//                 fixed4 red = fixed4(1, 0, 0, 1);
//                 fixed4 black = fixed4(0, 0, 0, 1);
//                 // float dist = distance(cameraworldspaceposition, vertexworldspaceposition);
                

                
//                 float maxDist = 100;
//                 float dist = length(-i.cameraDistance.z)/_MaxDist;

//                 fixed4 blend = mix(i.color, _BgColor ,getFogFactor(-i.cameraDistance.z));
//                 return blend;

                
                

//                 // return float4(-(i.cameraDistance).length, 1);
//                 // if(dist>0.1) {
//                 //     return col;
//                 // } else {
//                 //     return red;
//                 // }
//                 // return dist;
//             }
//             ENDCG
//         }
//     }
// }









//     SubShader {
//         Pass {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
            
//             //Needed for fog variation to be compiled.
//             #pragma multi_compile_fog /

//             #include "UnityCG.cginc"

//             struct vertexInput {
//                 float4 vertex : POSITION;
//                 float4 texcoord0 : TEXCOORD0;
//             };

//             struct fragmentInput{
//                 float4 position : SV_POSITION;
//                 float4 texcoord0 : TEXCOORD0;
                
//                 //Used to pass fog amount around number should be a free texcoord.
//                 UNITY_FOG_COORDS(1)
//             };

//             fragmentInput vert(vertexInput i){
//                 fragmentInput o;
//                 o.position = UnityObjectToClipPos (i.vertex);
//                 o.texcoord0 = i.texcoord0;
                
//                 //Compute fog amount from clip space position.
//                 UNITY_TRANSFER_FOG(o,o.position);
//                 return o;
//             }

//             fixed4 frag(fragmentInput i) : SV_Target {
//                 fixed4 color = fixed4(i.texcoord0.xy,0,0);
                
//                 //Apply fog (additive pass are automatically handled)
//                 UNITY_APPLY_FOG(i.fogCoord, color); 
                
//                 //to handle custom fog color another option would have been 
//                 //#ifdef UNITY_PASS_FORWARDADD
//                 //  UNITY_APPLY_FOG_COLOR(i.fogCoord, color, float4(0,0,0,0));
//                 //#else
//                 //  fixed4 myCustomColor = fixed4(0,0,1,0);
//                 //  UNITY_APPLY_FOG_COLOR(i.fogCoord, color, myCustomColor);
//                 //#endif
                
//                 return color;
//             }
//             ENDCG
//         }
//     }
// }