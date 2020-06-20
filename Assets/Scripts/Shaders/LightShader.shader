// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/LightShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_EmissionLM("Emmision (Lightmapper)", Float) = 0
		_angle("Angle", Float) = 1.57
		_radius("Radius", Float) = 2.0
		_dir("Direction", Vector) = (1,1,0,0)
		_origin("Origin", Vector) = (0,0,0,0)
	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Blend One One
			AlphaTest Greater .01
			ColorMask RGB
			Lighting Off
			ZWrite Off
			Fog { Color(0, 0, 0, 0)}

			Pass{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				// Use shader model 3.0 target, to get nicer looking lighting
				#pragma target 3.0

				sampler2D _MainTex;

				struct v2f {
					float4 pos : SV_POSITION;
					float2 tex : TEXCOORD0;
					float3 lpos : TEXCOORD2;
				};

				fixed4 _Color;
				float _radius;
				float _angle;
				float2 _dir;
				float2 _origin;

				// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
				// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
				// #pragma instancing_options assumeuniformscaling
				UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
				UNITY_INSTANCING_BUFFER_END(Props)

				v2f vert(appdata_full input)
				{
					v2f o;
					fixed4 pos = input.vertex;

					o.lpos = pos - float3(_origin.x, _origin.y, 0);
					o.pos = UnityObjectToClipPos(pos);
					o.tex = input.texcoord;
					return o;
				}

				fixed4 frag(v2f input) : COLOR0
				{
					float2 fragDir = normalize(input.lpos);
					float theta = acos(dot(_dir, fragDir));
					
					if (theta < _angle/2 && length(input.lpos) < _radius) {
						return fixed4(1, 0, 0, 1);
					}
					else {
						return fixed4(0, 0, 0, 0);
					}

				}

			ENDCG
			}


		}
		//FallBack "Diffuse"
}
