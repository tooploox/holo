Shader "Holo/testGeometry2"
{
	Properties
	{
		_MainTex("FX (RGB)", 2D) = "black" {}
		_WeightFactor("Weight", Range(.0,2.0)) = 0.3
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
				float2 uv : TEXCOORD0;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 col : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _WeightFactor;

			v2g vert(appdata_base v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.normal = v.normal;
				return o;
			}

			[maxvertexcount(24)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> tristream)
			{
				g2f o;
				
				float3 eyePosition = UnityObjectToViewPos(IN[0].vertex.xyz);
				float directionToCamera = -normalize(eyePosition);
				
				float3 normalFace = normalize(cross(normalize(IN[0].vertex), directionToCamera));				
				float3 crossNormalFace = normalize(cross(normalFace, normalize(IN[0].vertex)));

				fixed4 color = fixed4(normalize(IN[0].normal), 1);

				
			//-----
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(normalFace, 0) * _WeightFactor); //1
				o.uv = IN[0].uv;
				o.col = color;
				tristream.Append(o);
				
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(crossNormalFace, 0) * _WeightFactor); //2
				o.uv = IN[0].uv;
				o.col = color;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalFace, 0) * _WeightFactor); //3
				o.uv = IN[0].uv;
				o.col = color;
				tristream.Append(o);
				
				tristream.RestartStrip();
				
			//-----
				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalFace, 0) * _WeightFactor); //3
				o.uv = IN[0].uv;
				o.col = color;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(crossNormalFace, 0) * _WeightFactor); //4
				o.uv = IN[0].uv;
				o.col = color;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(normalFace, 0) * _WeightFactor); //1
				o.uv = IN[0].uv;
				o.col = color;
				tristream.Append(o);
				
				tristream.RestartStrip();
				
			}
			
			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * i.col;
				col.a = i.col.a;
				return col;
			}
			ENDCG
		}
	}
}