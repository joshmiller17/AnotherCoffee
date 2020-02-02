// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Unlit/TextShakeShader" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_DisplacementTex("Displacement Texture", 2D) = "white" {}
	}

		SubShader{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			LOD 100

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma multi_compile_fog

					#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
					};

					struct v2f {
						float4 vertex : SV_POSITION;
						half2 texcoord : TEXCOORD0;
						UNITY_FOG_COORDS(1)
					};

					sampler2D _MainTex;
					sampler2D _DisplacementTex;
					float4 _MainTex_ST;

					/* Uniforms are values passed to the shader by the program/CPU */
					uniform float _DisplacementSeed;
					uniform half _OffsetAmount; // = 0.2;
					uniform half _ShiftIntensity; // = 0.035;

					/* method for displacing a UV */
					half2 displace(half2 uv, float seed)
					{
						half2 offset = half2(
							sin(seed)		* _OffsetAmount,
							cos(seed + 0.2)	* _OffsetAmount
						);

						half4 map_uv = tex2D(_DisplacementTex, uv + offset);

						half2 new_uv = half2(
							uv.x + (map_uv[0] * _ShiftIntensity),
							uv.y + (map_uv[2] * _ShiftIntensity)
						);
						return new_uv;
					}


					v2f vert(appdata_t v)
					{
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						UNITY_TRANSFER_FOG(o,o.vertex);
						return o;
					}

					fixed4 frag(v2f i) : SV_Target
					{
						fixed4 col = tex2D(_MainTex, displace( i.texcoord, _DisplacementSeed ) );
						UNITY_APPLY_FOG(i.fogCoord, col);
						return col;
					}
				ENDCG
			}
	}

}