// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TargetShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _EmissionLM ("Emmision (Lightmapper)", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		//BlendOp RevSub
		BlendOp Sub
		Blend One One

		AlphaTest Greater .01
		ColorMask RGB
		Lighting Off 
		ZWrite Off 
		Fog { Color (0, 0, 0, 0)}
		//Fog { Mode Off}
		
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

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert(appdata_full input)
			{
				v2f output;
				fixed4 pos = input.vertex;

				output.lpos = input.vertex;
				output.pos = UnityObjectToClipPos(pos);
				output.tex = input.texcoord;
				return output;
			}

			fixed4 frag(v2f input) : COLOR0
			{
				fixed4 output = fixed4(0,0,0,0);
				//output.a = 1;
				output.rgb = _Color.xyz;
				//output.a = 1 / (1 + (input.lpos.x * input.lpos.x));
				output.a = _Color.a;

				return output;
			}

		ENDCG
		}

        
    }
    //FallBack "Diffuse"
}
