Shader "Projector/AlphaBlend" {
	Properties{
		_ShadowTex("Captured", 2D) = "white" {}
		_BackgroundTex("Background", 2D) = "white" {}
		_FalloffTex("FallOff", 2D) = "white" {}
		_TextureWidth("TexWidth", int) = 8000
		_TextureHeight("TexHeight", int) = 8000
		_WaveEffect("WaveEffect", Range(0,1)) = 0
		_CutoutXStart("Start X", Range(1,8000)) = 50
		_CutoutXEnd("End X", Range(1, 8000)) = 100
		_CutoutYStart("Start Y", Range(1, 8000)) = 50
		_CutoutYEnd("End Y", Range(1, 8000)) = 100
	}
		Subshader{
			Tags {"Queue" = "Transparent"}

			Lighting Off

			Pass {	
				//ZWrite Off
				ColorMask RGB
				Blend SrcAlpha OneMinusSrcAlpha
				Offset -1, -1

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata {
					float4 vertex : POSITION;
					float4 uv : TEXCOORD0;
				};

				struct v2f {
					float4 uvShadow : TEXCOORD0;
					float4 uvFalloff : TEXCOORD1;
					float4 pos : SV_POSITION;
				};

				float4x4 unity_Projector;
				float4x4 unity_ProjectorClip;
				float _CutoutXStart;
				float _CutoutXEnd;
				float _CutoutYStart;
				float _CutoutYEnd;
				int _TextureWidth;
				int _TextureHeight;

				sampler2D _ShadowTex;
				sampler2D _FalloffTex;

				v2f vert(appdata v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uvShadow = mul(unity_Projector, v.vertex);
					o.uvFalloff = mul(unity_ProjectorClip, v.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float2 uv = i.uvShadow.xy / i.uvShadow.w;
					float xStart = _CutoutXStart / _TextureWidth;
					float xEnd = _CutoutXEnd / _TextureWidth;
					float yStart = _CutoutYStart / _TextureHeight;
					float yEnd = _CutoutYEnd / _TextureHeight;

					fixed4 texS = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
					fixed4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));

					fixed4 res = texS;
					res.a *= texF.a;
					res.a *= step(xStart, uv.x) * step(uv.x, xEnd) 
						  * step(yStart, uv.y) * step(uv.y, yEnd);

					return res;
				}
				ENDCG
			}
	}
}