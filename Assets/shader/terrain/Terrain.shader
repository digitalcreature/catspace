Shader "Space/Terrain" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 0.5)

		_StencilReference ("Mask Reference Value", Int) = 16

	}


	SubShader {

		Stencil {
			Ref [_StencilReference]
			Comp Greater
		}

		Tags { "RenderType"="Opaque" }

		CGPROGRAM

		#pragma surface surf Standard addshadow fullforwardshadows

		#include "UnityCG.cginc"

		sampler2D _MainTex;
    fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input i, inout SurfaceOutputStandard o) {
			o.Albedo = _Color.rgb * tex2D(_MainTex, i.uv_MainTex).rgb;
		}

		ENDCG

	}
}
