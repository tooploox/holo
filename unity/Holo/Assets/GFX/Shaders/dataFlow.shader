Shader "Holo/dataflow"
{
	Properties
	{
		_MainTex("Ground (RGB)", 2D) = "white" {}
		_WeightFactor("Line weight", Range(.01,.1)) = 0.01
		_LengthFactor("Line weight", Range(.01,.1)) = 0.01
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
			float _LengthFactor;

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
				float3 normalFace = normalize(cross(normalize(IN[0].normal), _WorldSpaceCameraPos));
				
				for (int i = 0; i < 1; i++)
				{
					o.pos = UnityObjectToClipPos(IN[i].vertex);
					o.uv = IN[i].uv;
					o.col = fixed4(0., 0., 0., 1.);
					tristream.Append(o);

					o.pos = UnityObjectToClipPos(IN[i].vertex + float4(normalFace, 0) * _WeightFactor);
					o.uv = IN[i].uv;
					o.col = fixed4(0., 0., 0., 1.);
					tristream.Append(o);

					o.pos = UnityObjectToClipPos(IN[i].vertex + float4(normalize(IN[i].normal) * _LengthFactor, 1));
					o.uv = IN[i].uv;
					o.col = fixed4(1., 1., 1., 1.);
					tristream.Append(o);

					tristream.RestartStrip();
				}
			}

			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) * i.col;
				return col;
			}
			ENDCG
		}
	}
}