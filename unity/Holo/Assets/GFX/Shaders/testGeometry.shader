Shader "Holo/testGeometry"
{
	Properties
	{
		_MainTex("FX (RGB)", 2D) = "white" {}
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

			v2g vert(appdata_base v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			[maxvertexcount(24)]
			void geom(triangle v2g IN[3], inout TriangleStream<g2f> tristream)
			{
				g2f o;

				o.pos = UnityObjectToClipPos(IN[0].vertex); //1
				o.uv = IN[0].uv;
				o.col = fixed4(1,1,1,1);
				tristream.Append(o);
				
				o.pos = UnityObjectToClipPos(IN[1].vertex); //2
				o.uv = IN[1].uv;
				o.col = fixed4(1,1,1,1);
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[2].vertex); //3
				o.uv = IN[2].uv;
				o.col = fixed4(1,1,1,1);
				tristream.Append(o);
				
				tristream.RestartStrip();
			}
			
			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.a = i.col.a;
				return col;
			}
			ENDCG
		}
	}
}