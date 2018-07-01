Shader "FX/IcoStar" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Alpha ("Alpha", Range(0, 1)) = 0.5
		_FresnelPower ("Fresnel Power", Float) = 5
		_Specular ("Specular", Color) = (1, 1, 1, 1)
		_Smoothness ("Smoothness", Range(0, 1)) = 0.5
		_RimColor ("Rimlight Color", Color) = (1, 1, 1, 1)
		_RimPower ("Rimlight Power", Float) = 5
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		// ZWrite Off

		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM

		#pragma surface surf StandardSpecular alpha

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		fixed _Alpha;
		float _FresnelPower;
		fixed4 _Specular;
		fixed _Smoothness;

		fixed4 _RimColor;
		float _RimPower;

		struct Input {
			float2 uv_MainTex;
			float3 worldNormal;
			float3 worldPos;
		};

		void surf(Input i, inout SurfaceOutputStandardSpecular o) {
			float3 normalDir = i.worldNormal;
			float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
			float fresnel = 1 - saturate(dot(viewDir, i.worldNormal));

			fixed3 color = tex2D(_MainTex, i.uv_MainTex).rgb;

			o.Albedo = color;
			o.Specular = _Specular.rgb;
			o.Smoothness = _Smoothness;

			o.Alpha = _Alpha * (pow(1 - fresnel, _FresnelPower));
			o.Emission = _RimColor.xyz * pow(fresnel, _RimPower);
		}


		ENDCG
	}
}
