Shader "Holo/DataFlow"
{
	Properties
	{
		_MainTex("FX (RGB)", 2D) = "black" {}
		_ColorMap("Color Map (RGB)", 2D) = "black" {}
		_ScaleFactor("Scale Factor", Range(.0,5.0)) = 2.0
		_DisplayMode("Display Mode", Int) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#include "UnityCG.cginc"

			struct v2g
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 col : COLOR;
				float4 tan : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _ColorMap;
			float4 _ColorMap_ST;
			float _ScaleFactor;
			float _DisplayMode;

			v2g vert(appdata_tan v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.normal = v.normal;
				o.tangent = v.tangent;
				return o;
			}

			[maxvertexcount(24)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> tristream)
			{
				g2f o;
				
				float3 nNormal = normalize(IN[0].normal); //normalized normal
				float3 eyePosition = UnityObjectToViewPos(IN[0].vertex.xyz);
				float directionToCamera = -normalize(eyePosition);
				float3 normalFace = normalize(cross(nNormal, directionToCamera));
				
				float3 crossNormalFace = normalize(cross(normalFace, nNormal));
				float2 offset = -float2(0., _Time.z);
				
				fixed4 endColor = fixed4(1,1,1,1);
				fixed4 startColor = fixed4(.8, .8, .8, 1.);
				
				float _WeightFactor = _ScaleFactor * 0.45;
				
			//-----
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(normalFace, 0) * _WeightFactor); //1
				o.uv = float2(0.,0.) + offset;
				o.col = startColor;
				o.tan = IN[0].tangent;
				tristream.Append(o);
				
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(crossNormalFace, 0) * _WeightFactor); //2
				o.uv = float2(1.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalize(IN[0].normal) * _ScaleFactor, 1));
				o.uv = float2(.5,2.) + offset;
				o.col = endColor;
				tristream.Append(o);
				
				tristream.RestartStrip();
				
			//-----
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(crossNormalFace, 0) * _WeightFactor); //2
				o.uv = float2(0.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalFace, 0) * _WeightFactor); //3
				o.uv = float2(1.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalize(IN[0].normal) * _ScaleFactor, 1));
				o.uv = float2(.5,2.) + offset;
				o.col = endColor;
				tristream.Append(o);
				
				tristream.RestartStrip();
				
			//-----
				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalFace, 0) * _WeightFactor); //3
				o.uv = float2(0.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(crossNormalFace, 0) * _WeightFactor); //4
				o.uv = float2(1.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalize(IN[0].normal) * _ScaleFactor, 1));
				o.uv = float2(.5,2.) + offset;
				o.col = endColor;
				tristream.Append(o);
				
				tristream.RestartStrip();
				
			//-----
				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(crossNormalFace, 0) * _WeightFactor); //4
				o.uv = float2(0.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);
				
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(normalFace, 0) * _WeightFactor); //1
				o.uv = float2(1.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalize(IN[0].normal) * _ScaleFactor, 1));
				o.uv = float2(.5,2.) + offset;
				o.col = endColor;
				tristream.Append(o);
				
				tristream.RestartStrip();
			}
			
			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 col = i.col;
				col.a = i.col.a;
				
				if(_DisplayMode == 1)
					col = tex2D(_ColorMap, float2((i.tan.x + 1) / 2,0)) * i.col;
				else if (_DisplayMode == 2)
					col = tex2D(_ColorMap, float2((i.tan.y + 1) / 2,0)) * i.col;
				
				col += tex2D(_MainTex, i.uv);
				
				return col;
			}
			ENDCG
		}
	}
}
