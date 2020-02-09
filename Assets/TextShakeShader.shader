// shaky text shader for Unity, feb 2020

Shader "Unlit/TextShakeShader" {
    Properties {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Text Color", Color) = (1,1,1,1)
        _DisplacementTex("Displacement Texture", 2D) = "white" {}
    }

    SubShader {

        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Lighting Off Cull Off ZTest Always ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            /* Textures */
            sampler2D _MainTex;
            sampler2D _DisplacementTex;

            /* Uniforms managed by Unity */
            uniform float4 _MainTex_ST;
            uniform fixed4 _Color;
            uniform float4 _MainTex_TexelSize;

            /* Uniforms manually passed in by developer */
  					uniform float _DisplacementSeed;
  					uniform half _OffsetAmount;
  					uniform half _ShiftIntensity;
                    uniform float _SafeMode;

            /* Method for displacing a UV */
  					half2 displace(half2 uv, float seed)
  					{
  						half2 offset = half2(
  							sin(seed)  * _OffsetAmount,
  							cos(seed)  * _OffsetAmount
  						);

  						half4 map_uv = tex2D(_DisplacementTex, uv + offset);

              /* Reduce jitter at incredibly low map_UV levels to reduce font-wide offsetting */
              map_uv[0] = map_uv[0] * map_uv[0];
              map_uv[2] = map_uv[2] * map_uv[2];

  						half2 new_uv = half2(
  							uv.x + (map_uv[0] * _ShiftIntensity * (_MainTex_TexelSize.w*0.003) ),
  							uv.y + (map_uv[2] * _ShiftIntensity * (_MainTex_TexelSize.z*0.003) )
  						);
  						return new_uv;
  					}

            /* Vertex Shader */
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                return o;
            }

            /* Fragment Shader */
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color;
                /* Safe mode "clamps" the shake to be within the bounds of the original UV */
                if (_SafeMode > 0.5) {
                  col.a *= (
                    tex2D(_MainTex, displace(i.texcoord, _DisplacementSeed)).a
                    * tex2D(_MainTex, i.texcoord).a
                  );
                } else { // Without safe mode, there is no clamping, and clipping of neighboring glyphs may occur
                  col.a *= ( tex2D(_MainTex, displace(i.texcoord, _DisplacementSeed)).a );
                }
                return col;
            }
            ENDCG
        }
    }
}
