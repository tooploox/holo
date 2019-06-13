Shader "Holo/dataflow"
{
	Properties
	{
		_MainTex("Ground (RGB)", 2D) = "white" {}
		_LengthFactor("Line length", Range(.01,.1)) = 0.01
		_WeightFactor("Line weight", Range(.001,.02)) = 0.01		
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
				
				//float3 normalFace = normalize(cross(normalize(IN[0].normal), _WorldSpaceCameraPos));
				
				float3 nNormal = normalize(IN[0].normal); //normalized normal
				float3 eyePosition = UnityObjectToViewPos(IN[0].vertex.xyz);
				float directionToCamera = -normalize(eyePosition);
				float3 normalFace = normalize(cross(nNormal, directionToCamera));
				
				float3 crossNormalFace = normalize(cross(normalFace, nNormal));
				
				float4 cNormal = (fixed4(nNormal, 1) + fixed4(1,1,1,0)) * fixed4(.5,.5,.5,1);				
				float2 offset = -float2(0., _Time.z);
				
			//o.uv = IN[0].uv;
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(normalFace, 0) * _WeightFactor); //1
				o.uv = float2(0.,0.) + offset;
				o.col = fixed4(.8, .8, .8, 0.) * cNormal;
				tristream.Append(o);
				
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(crossNormalFace, 0) * _WeightFactor); //2
				o.uv = float2(1.,0.) + offset;
				o.col = fixed4(.8, .8, .8, 0.) * cNormal;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalize(IN[0].normal) * _LengthFactor, 1));
				o.uv = float2(.5,3.) + offset;
				o.col = fixed4(1., 1., 1., 1.);
				tristream.Append(o);
				
				tristream.RestartStrip();
				
			//o.uv = IN[0].uv;
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(crossNormalFace, 0) * _WeightFactor); //2
				o.uv = float2(0.,0.) + offset;
				o.col = fixed4(.8, .8, .8, 0.) * cNormal;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalFace, 0) * _WeightFactor); //3
				o.uv = float2(1.,0.) + offset;
				o.col = fixed4(.8, .8, .8, 0.) * cNormal;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalize(IN[0].normal) * _LengthFactor, 1));
				o.uv = float2(.5,3.) + offset;
				o.col = fixed4(1., 1., 1., 1.);
				tristream.Append(o);
				
				tristream.RestartStrip();
				
			//o.uv = IN[0].uv;
				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalFace, 0) * _WeightFactor); //3
				o.uv = float2(0.,0.) + offset;
				o.col = fixed4(.8, .8, .8, 0.) * cNormal;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(crossNormalFace, 0) * _WeightFactor); //4
				o.uv = float2(1.,0.) + offset;
				o.col = fixed4(.8, .8, .8, 0.) * cNormal;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalize(IN[0].normal) * _LengthFactor, 1));
				o.uv = float2(.5,3.) + offset;
				o.col = fixed4(1., 1., 1., 1.);
				tristream.Append(o);
				
				tristream.RestartStrip();
				
			//o.uv = IN[0].uv;
				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(crossNormalFace, 0) * _WeightFactor); //4
				o.uv = float2(0.,0.) + offset;
				o.col = fixed4(.1, .1, .1, 0.) * cNormal;
				tristream.Append(o);
				
				o.pos = UnityObjectToClipPos(IN[0].vertex - float4(normalFace, 0) * _WeightFactor); //1
				o.uv = float2(1.,0.) + offset;
				o.col = fixed4(1.8, 1.8, 1.8, 0.) * cNormal;
				tristream.Append(o);

				o.pos = UnityObjectToClipPos(IN[0].vertex + float4(normalize(IN[0].normal) * _LengthFactor, 1));
				o.uv = float2(.5,3.) + offset;
				o.col = fixed4(1., 1., 1., 1.);
				tristream.Append(o);
				
				tristream.RestartStrip();
			}
			
			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv) + i.col;
				col.a = i.col.a;
				return col;
			}
			ENDCG
		}
	}
}