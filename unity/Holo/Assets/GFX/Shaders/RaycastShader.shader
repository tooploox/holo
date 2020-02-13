Shader "RaycastShader"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
	    _LeftEye("LeftEye", Vector) = (0,0,0,0)
		_RightEye("RightEye", Vector) = (0,0,0,0)
		
		_Channel1("Channel1", Color) = (1,0,0,1)
		_Channel2("Channel2", Color) = (0,1,0,1)
		_Channel3("Channel3", Color) = (0,0,1,1)
		_Channel4("Channel4", Color) = (0.5,0.5,0.5,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uvw : TEXCOORD0;
            };

            struct v3f
            {
                float3 uvw : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler3D _MainTex;
			float4 _LeftEye;
			float4 _RightEye;
			float4 _MainTex_ST;

			fixed4 _Channel1;
			fixed4 _Channel2;
			fixed4 _Channel3;
			fixed4 _Channel4;
			
            v3f vert (appdata v) {
                v3f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uvw = v.vertex.xyz;
                return o;
            }

			fixed4 frag(v3f i) : SV_Target{
				const int iter = 50;
				float3 acc = 0;
				float3 origin = i.uvw;
				float3 dir = normalize(origin - (unity_StereoEyeIndex ? _RightEye.xyz : _LeftEye.xyz));
				// float3 camera_pos = _WorldSpaceCameraPos;
				// #if defined(USING_STEREO_MATRICES)
				// 	camera_pos = unity_StereoWorldSpaceCameraPos[unity_StereoEyeIndex];
				// #endif
				// float3 dir = normalize(origin - camera_pos);

				float z_div = max(abs(dir.z), 0.1) * 25.0;
				dir /= z_div;
				float opacity = 1.0;
				for (int it = 0; it < iter; ++it) {
					fixed4 color = tex3D(_MainTex, origin + float3(0.5, 0.5, 0.5));
					float amount = 0.3;
					// todo: this could be a matrix multiply
					acc += amount * _Channel1.rgb * color.x *_Channel1.a;
					acc += amount * _Channel2.rgb * color.y *_Channel2.a;
					acc += amount * _Channel3.rgb * color.z *_Channel3.a;
					acc += amount * _Channel4.rgb * color.w *_Channel4.a;
                    //acc += amount * color.rgba;

//					acc += 0.01 * v * opacity;
					//acc *= (1.0 - opacity);
//					opacity *= 1.0 - (0.01 * v);
					origin += dir;
					if (any(origin > float3(0.5, 0.5, 0.5))) break;
					if (any(origin < float3(-0.5, -0.5, -0.5))) break;
				//	if (opacity < 0.01) break;
				}

//				float v = acc;
				fixed4 col = fixed4(acc, 1.0);
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
