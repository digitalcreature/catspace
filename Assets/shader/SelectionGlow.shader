Shader "FX/SelectionGlow" {
	Properties {
		_GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
		_GlowPowerHigh ("Glow Power High", Float) = 5
		_GlowPowerLow ("Glow Power Low", Float) = 10
		_BlinkRate ("Blink Rate", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 100

		// ZTest Always
		// ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			half4 _GlowColor;
			float _GlowPowerHigh;
			float _GlowPowerLow;
			float _BlinkRate;

			struct v2f {
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float4 posWorld : TEXCOORD0;
			};

			v2f vert(appdata_base v) {
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);

				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
				o.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);

				return o;
			}

			half4 frag(v2f i) : SV_Target {
				float3 normalDir = i.normal;
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				float power = lerp(_GlowPowerLow, _GlowPowerHigh, (sin(_Time.y * _BlinkRate) + 1) / 2.0f);
				float rim = 1 - saturate(dot(viewDir, normalDir));
				rim = pow(rim, power);
				return float4(_GlowColor.xyz, _GlowColor.w * rim);
			}
			ENDCG
		}
	}
}
