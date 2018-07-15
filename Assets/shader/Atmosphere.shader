﻿Shader "Space/Atmosphere" {
	Properties {
		_MainTex ("Sky Gradient (Space)", 2D) = "white" {}
		_MainTex_Surface ("Sky Gradient (Surface)", 2D) = "white" {}
		_SurfaceFade ("Surface Fade", Range(0, 1)) = 0			// 1 when on surface, 0 when in space
		_Color ("Sky Color", Color) = (1, 1, 1, 1)
		_Radius ("Radius", Float) = 1000
		_FadeMinDistance ("Min Fade Distance", Float) = 0.125
		_FadeMaxDistance ("Max Fade Distance", Float) = 1.75
		_SunDirection ("Sun Direction", Vector) = (1, 0, 0, 0)
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }

		// ZWrite Off

		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _MainTex_Surface;
			fixed4 _Color;
			half _SurfaceFade;
			float _Radius;
			half _FadeMinDistance;
			half _FadeMaxDistance;
			float4 _SunDirection;

			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : NORMAL;
				float fade : TEXCOORD0;
			};

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				float3 farPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float3 farNormal = mul(unity_ObjectToWorld, v.normal).xyz;
				float3 farToCam = _WorldSpaceCameraPos - farPos;
				float distance = length(farToCam);
				float angle = 3.14159265 - 2 * acos(dot(normalize(farToCam), normalize(farNormal)));
				float chord = 2 * _Radius * sin(angle / 2);
				if (chord < distance) {
					distance = chord;
				}
				o.fade = smoothstep(_Radius * _FadeMinDistance, _Radius * _FadeMaxDistance, distance);
				o.worldNormal = farNormal;
				return o;
			}

			half4 frag(v2f i) : SV_Target {
				float brightness = (dot(normalize(_SunDirection), normalize(i.worldNormal)) + 1) / 2;
				float2 uv = float2(1 - brightness, 1 - i.fade);
				half4 space = tex2D(_MainTex, uv);
				half4 surface = tex2D(_MainTex_Surface, uv);
				return lerp(space, surface, _SurfaceFade) * _Color;
			}
			ENDCG
		}
	}
}
