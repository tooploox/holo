Shader "Holo/DataFlow"
{
	Properties
	{
		_MainTex("FX (RGB)", 2D) = "black" {}
		_ColorMap("Color Map (RGB)", 2D) = "black" {}
		_ScaleFactor("Scale Factor", Range(.0,5.0)) = 2.0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Cull Off

		Pass
		{
			CGPROGRAM
			// See https://docs.unity3d.com/Manual/SL-MultipleProgramVariants.html
			// about Unity "shader variants".
			#pragma shader_feature CLIPPING_OFF CLIPPING_ON

			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#include "UnityCG.cginc"

			#ifdef CLIPPING_ON
			float4 _Plane;
			#endif

			#define DISPLAY_MODE_1

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

				if(length(IN[0].normal) == 0){
					return;
				}

				float3 nNormal = normalize(IN[0].normal); //normalized normal
				float3 eyePosition = UnityObjectToViewPos(IN[0].vertex.xyz);
				float directionToCamera = -normalize(eyePosition);
				float3 normalFace = normalize(cross(nNormal, directionToCamera));

				float3 crossNormalFace = normalize(cross(normalFace, nNormal));
				float2 offset = -float2(0., _Time.z);

				fixed4 endColor = fixed4(1,1,1,1);
				fixed4 startColor = fixed4(.8, .8, .8, 1.);

				float _WeightFactor = _ScaleFactor * 0.45;
				_ScaleFactor *= length(IN[0].normal);

				/* We perform clipping in the geometry shader,
				   using the "main vertex" from IN[0].vertex as the position that
				   determines whether to clip.
				   This works sensibly for small _ScaleFactor values.

				   Why not do this in fragment shader (which would be more correct?):

				   - Because calling clip() or discard in fragment shader causes
				     a bug: instead of filled, the generated geometry is displayed
				     as wireframe.
				     Is it a bug of Unity, or Direct3D? Unknown.

				   - As a side effect, this is also faster. No need to calculate vertexWorld
				     for each generated vertex, no need to do clip in fragment shader.
				*/
				#ifdef CLIPPING_ON
				float4 mainVertexWorld = mul(unity_ObjectToWorld, IN[0].vertex);
				float distance = dot(mainVertexWorld, _Plane.xyz);
				distance = distance + _Plane.w;
				// discard surface above plane
				if (-distance < 0) {
					return;
				}
				#endif

				float4 vertexObject;

			//-----
				vertexObject = IN[0].vertex - float4(normalFace, 0) * _WeightFactor;
				o.pos = UnityObjectToClipPos(vertexObject); //1
				o.uv = float2(0.,0.) + offset;
				o.col = startColor;
				o.tan = IN[0].tangent;
				tristream.Append(o);

				vertexObject = IN[0].vertex - float4(crossNormalFace, 0) * _WeightFactor;
				o.pos = UnityObjectToClipPos(vertexObject); //2
				o.uv = float2(1.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				vertexObject = IN[0].vertex + float4(normalize(IN[0].normal) * _ScaleFactor, 1);
				o.pos = UnityObjectToClipPos(vertexObject);
				o.uv = float2(.5,2.) + offset;
				o.col = endColor;
				tristream.Append(o);

				tristream.RestartStrip();

			//-----
				vertexObject = IN[0].vertex - float4(crossNormalFace, 0) * _WeightFactor;
				o.pos = UnityObjectToClipPos(vertexObject); //2
				o.uv = float2(0.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				vertexObject = IN[0].vertex + float4(normalFace, 0) * _WeightFactor;
				o.pos = UnityObjectToClipPos(vertexObject); //3
				o.uv = float2(1.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				vertexObject = IN[0].vertex + float4(normalize(IN[0].normal) * _ScaleFactor, 1);
				o.pos = UnityObjectToClipPos(vertexObject);
				o.uv = float2(.5,2.) + offset;
				o.col = endColor;
				tristream.Append(o);

				tristream.RestartStrip();

			//-----
				vertexObject = IN[0].vertex + float4(normalFace, 0) * _WeightFactor;
				o.pos = UnityObjectToClipPos(vertexObject); //3
				o.uv = float2(0.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				vertexObject = IN[0].vertex + float4(crossNormalFace, 0) * _WeightFactor;
				o.pos = UnityObjectToClipPos(vertexObject); //4
				o.uv = float2(1.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				vertexObject = IN[0].vertex + float4(normalize(IN[0].normal) * _ScaleFactor, 1);
				o.pos = UnityObjectToClipPos(vertexObject);
				o.uv = float2(.5,2.) + offset;
				o.col = endColor;
				tristream.Append(o);

				tristream.RestartStrip();

			//-----
				vertexObject = IN[0].vertex + float4(crossNormalFace, 0) * _WeightFactor;
				o.pos = UnityObjectToClipPos(vertexObject); //4
				o.uv = float2(0.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				vertexObject = IN[0].vertex - float4(normalFace, 0) * _WeightFactor;
				o.pos = UnityObjectToClipPos(vertexObject); //1
				o.uv = float2(1.,0.) + offset;
				o.col = startColor;
				tristream.Append(o);

				vertexObject = IN[0].vertex + float4(normalize(IN[0].normal) * _ScaleFactor, 1);
				o.pos = UnityObjectToClipPos(vertexObject);
				o.uv = float2(.5,2.) + offset;
				o.col = endColor;
				tristream.Append(o);

				tristream.RestartStrip();
			}

			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 col = i.col;
				col.a = i.col.a;

				#ifdef DISPLAY_MODE_0
				col = i.tan * i.col;
				#endif
				#ifdef DISPLAY_MODE_1
				col = tex2D(_ColorMap, float2((i.tan.x + 1) / 2,0)) * i.col;
				#endif
				#ifdef DISPLAY_MODE_2
				col = tex2D(_ColorMap, float2((i.tan.y + 1) / 2,0)) * i.col;
				#endif

				col += tex2D(_MainTex, i.uv);

				return col;
			}
			ENDCG
		}
	}
}
