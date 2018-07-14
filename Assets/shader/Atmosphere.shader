Shader "Space/Atmosphere" {
	Properties {
		_MainTex ("Sky Gradient", 2D) = "white" {}
		_Color ("Sky Color", Color) = (1, 1, 1, 1)
		_FresnelPower ("Fresnel Power", Float) = 5
		_AlphaFade ("Alpha Fade", Float) = 1
		_FresnelFactor ("Fresnel Factor", Range(0, 1)) = 1
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }

		ZWrite Off

		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float _FresnelPower;
			float _AlphaFade;
			fixed _FresnelFactor;

			struct v2f {
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float4 worldPos : TEXCOORD0;
			};

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.normal = v.normal;
				return o;
			}

			half4 frag(v2f i) : SV_Target {
				fixed alpha;
				float3 worldNormal = normalize(mul(float4(i.normal, 0.0), unity_WorldToObject).xyz);
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				float fresnel = 1 - saturate(dot(viewDir, worldNormal));
				fresnel = lerp(0, pow(fresnel, _FresnelPower), _FresnelFactor);
				alpha = clamp(_Color.w - (fresnel * _AlphaFade), 0, 1);
				return half4(_Color.xyz, alpha);
			}
			ENDCG
		}
	}
}
