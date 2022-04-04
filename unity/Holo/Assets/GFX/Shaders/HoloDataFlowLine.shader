// DataFlow shader for Essential Vision ( https://www.essential-vision.org/ ).
// For copyright and license see https://github.com/tooploox/holo .
//
// Based on Mrtk2 standard shader, original copyright and license:
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.

Shader "Holo/DataFlowLine"
{
	Properties
	{
		// Main maps.
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex("Albedo", 2D) = "white" {}
		[Enum(AlbedoAlphaMode)] _AlbedoAlphaMode("Albedo Alpha Mode", Float) = 0 // "Transparency"
		[Toggle] _AlbedoAssignedAtRuntime("Albedo Assigned at Runtime", Float) = 0.0
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		_Metallic("Metallic", Range(0.0, 1.0)) = 0.0
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
		[Toggle(_CHANNEL_MAP)] _EnableChannelMap("Enable Channel Map", Float) = 0.0
		[NoScaleOffset] _ChannelMap("Channel Map", 2D) = "white" {}
		[Toggle(_EMISSION)] _EnableEmission("Enable Emission", Float) = 0.0
		[HDR]_EmissiveColor("Emissive Color", Color) = (0.0, 0.0, 0.0, 1.0)
		[Toggle(_TRIPLANAR_MAPPING)] _EnableTriplanarMapping("Triplanar Mapping", Float) = 0.0
		[Toggle(_LOCAL_SPACE_TRIPLANAR_MAPPING)] _EnableLocalSpaceTriplanarMapping("Local Space", Float) = 0.0
		_TriplanarMappingBlendSharpness("Blend Sharpness", Range(1.0, 16.0)) = 4.0

			// Rendering options.
			[Toggle(_DIRECTIONAL_LIGHT)] _DirectionalLight("Directional Light", Float) = 1.0
			[Toggle(_SPECULAR_HIGHLIGHTS)] _SpecularHighlights("Specular Highlights", Float) = 1.0
			[Toggle(_SPHERICAL_HARMONICS)] _SphericalHarmonics("Spherical Harmonics", Float) = 0.0
			[Toggle(_REFLECTIONS)] _Reflections("Reflections", Float) = 0.0
			[Toggle(_REFRACTION)] _Refraction("Refraction", Float) = 0.0
			_RefractiveIndex("Refractive Index", Range(0.0, 3.0)) = 0.0
			[Toggle(_RIM_LIGHT)] _RimLight("Rim Light", Float) = 0.0
			_RimColor("Rim Color", Color) = (0.5, 0.5, 0.5, 1.0)
			_RimPower("Rim Power", Range(0.0, 8.0)) = 0.25
			[Toggle(_VERTEX_COLORS)] _VertexColors("Vertex Colors", Float) = 0.0
			[Toggle(_VERTEX_EXTRUSION)] _VertexExtrusion("Vertex Extrusion", Float) = 0.0
			_VertexExtrusionValue("Vertex Extrusion Value", Float) = 0.0
			[Toggle(_VERTEX_EXTRUSION_SMOOTH_NORMALS)] _VertexExtrusionSmoothNormals("Vertex Extrusion Smooth Normals", Float) = 0.0
			_BlendedClippingWidth("Blended Clipping With", Range(0.0, 10.0)) = 1.0
			[Toggle(_CLIPPING_BORDER)] _ClippingBorder("Clipping Border", Float) = 0.0
			_ClippingBorderWidth("Clipping Border Width", Range(0.0, 1.0)) = 0.025
			_ClippingBorderColor("Clipping Border Color", Color) = (1.0, 0.2, 0.0, 1.0)
			[Toggle(_NEAR_PLANE_FADE)] _NearPlaneFade("Near Plane Fade", Float) = 0.0
			[Toggle(_NEAR_LIGHT_FADE)] _NearLightFade("Near Light Fade", Float) = 0.0
			_FadeBeginDistance("Fade Begin Distance", Range(0.0, 10.0)) = 0.85
			_FadeCompleteDistance("Fade Complete Distance", Range(0.0, 10.0)) = 0.5
			_FadeMinValue("Fade Min Value", Range(0.0, 1.0)) = 0.0

			// Fluent options.
			[Toggle(_HOVER_LIGHT)] _HoverLight("Hover Light", Float) = 1.0
			[Toggle(_HOVER_COLOR_OVERRIDE)] _EnableHoverColorOverride("Hover Color Override", Float) = 0.0
			_HoverColorOverride("Hover Color Override", Color) = (1.0, 1.0, 1.0, 1.0)
			[Toggle(_PROXIMITY_LIGHT)] _ProximityLight("Proximity Light", Float) = 0.0
			[Toggle(_PROXIMITY_LIGHT_COLOR_OVERRIDE)] _EnableProximityLightColorOverride("Proximity Light Color Override", Float) = 0.0
			[HDR]_ProximityLightCenterColorOverride("Proximity Light Center Color Override", Color) = (1.0, 0.0, 0.0, 0.0)
			[HDR]_ProximityLightMiddleColorOverride("Proximity Light Middle Color Override", Color) = (0.0, 1.0, 0.0, 0.5)
			[HDR]_ProximityLightOuterColorOverride("Proximity Light Outer Color Override", Color) = (0.0, 0.0, 1.0, 1.0)
			[Toggle(_PROXIMITY_LIGHT_SUBTRACTIVE)] _ProximityLightSubtractive("Proximity Light Subtractive", Float) = 0.0
			[Toggle(_PROXIMITY_LIGHT_TWO_SIDED)] _ProximityLightTwoSided("Proximity Light Two Sided", Float) = 0.0
			_FluentLightIntensity("Fluent Light Intensity", Range(0.0, 1.0)) = 1.0
			[Toggle(_ROUND_CORNERS)] _RoundCorners("Round Corners", Float) = 0.0
			_RoundCornerRadius("Round Corner Radius", Range(0.0, 0.5)) = 0.25
			_RoundCornerMargin("Round Corner Margin", Range(0.0, 0.5)) = 0.01
			[Toggle(_INDEPENDENT_CORNERS)] _IndependentCorners("Independent Corners", Float) = 0.0
			_RoundCornersRadius("Round Corners Radius", Vector) = (0.5 ,0.5, 0.5, 0.5)
			[Toggle(_BORDER_LIGHT)] _BorderLight("Border Light", Float) = 0.0
			[Toggle(_BORDER_LIGHT_USES_HOVER_COLOR)] _BorderLightUsesHoverColor("Border Light Uses Hover Color", Float) = 0.0
			[Toggle(_BORDER_LIGHT_REPLACES_ALBEDO)] _BorderLightReplacesAlbedo("Border Light Replaces Albedo", Float) = 0.0
			[Toggle(_BORDER_LIGHT_OPAQUE)] _BorderLightOpaque("Border Light Opaque", Float) = 0.0
			_BorderWidth("Border Width", Range(0.0, 1.0)) = 0.1
			_BorderMinValue("Border Min Value", Range(0.0, 1.0)) = 0.1
			_EdgeSmoothingValue("Edge Smoothing Value", Range(0.0, 0.2)) = 0.002
			_BorderLightOpaqueAlpha("Border Light Opaque Alpha", Range(0.0, 1.0)) = 1.0
			[Toggle(_INNER_GLOW)] _InnerGlow("Inner Glow", Float) = 0.0
			_InnerGlowColor("Inner Glow Color (RGB) and Intensity (A)", Color) = (1.0, 1.0, 1.0, 0.75)
			_InnerGlowPower("Inner Glow Power", Range(2.0, 32.0)) = 4.0
			[Toggle(_IRIDESCENCE)] _Iridescence("Iridescence", Float) = 0.0
			[NoScaleOffset] _IridescentSpectrumMap("Iridescent Spectrum Map", 2D) = "white" {}
			_IridescenceIntensity("Iridescence Intensity", Range(0.0, 1.0)) = 0.5
			_IridescenceThreshold("Iridescence Threshold", Range(0.0, 1.0)) = 0.05
			_IridescenceAngle("Iridescence Angle", Range(-0.78, 0.78)) = -0.78
			[Toggle(_ENVIRONMENT_COLORING)] _EnvironmentColoring("Environment Coloring", Float) = 0.0
			_EnvironmentColorThreshold("Environment Color Threshold", Range(0.0, 3.0)) = 1.5
			_EnvironmentColorIntensity("Environment Color Intensity", Range(0.0, 1.0)) = 0.5
			_EnvironmentColorX("Environment Color X (RGB)", Color) = (1.0, 0.0, 0.0, 1.0)
			_EnvironmentColorY("Environment Color Y (RGB)", Color) = (0.0, 1.0, 0.0, 1.0)
			_EnvironmentColorZ("Environment Color Z (RGB)", Color) = (0.0, 0.0, 1.0, 1.0)

				// Advanced options.
				[Enum(RenderingMode)] _Mode("Rendering Mode", Float) = 0                                     // "Opaque"
				[Enum(CustomRenderingMode)] _CustomMode("Mode", Float) = 0                                   // "Opaque"
				[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 1                 // "One"
				[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 0            // "Zero"
				[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation", Float) = 0                 // "Add"
				[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Depth Test", Float) = 4                // "LessEqual"
				[Enum(DepthWrite)] _ZWrite("Depth Write", Float) = 1                                         // "On"
				_ZOffsetFactor("Depth Offset Factor", Float) = 0                                             // "Zero"
				_ZOffsetUnits("Depth Offset Units", Float) = 0                                               // "Zero"
				[Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorWriteMask("Color Write Mask", Float) = 15 // "All"
				[Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2                     // "Back"
				_RenderQueueOverride("Render Queue Override", Range(-1.0, 5000)) = -1
				[Toggle(_INSTANCED_COLOR)] _InstancedColor("Instanced Color", Float) = 0.0
				[Toggle(_IGNORE_Z_SCALE)] _IgnoreZScale("Ignore Z Scale", Float) = 0.0
				[Toggle(_STENCIL)] _Stencil("Enable Stencil Testing", Float) = 0.0
				_StencilReference("Stencil Reference", Range(0, 255)) = 0
				[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComparison("Stencil Comparison", Int) = 0
				[Enum(UnityEngine.Rendering.StencilOp)]_StencilOperation("Stencil Operation", Int) = 0

				// HoloDataFlow specific options
				_ColorMap("Color Map (RGB)", 2D) = "black" {}
				_ScaleFactor("Scale Factor", Range(.0,5.0)) = 2.0
	}

		SubShader
				{
					Pass
					{
						Name "Main"
						Tags{ "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
						LOD 100
						Blend[_SrcBlend][_DstBlend]
						BlendOp[_BlendOp]
						ZTest[_ZTest]
						ZWrite[_ZWrite]
						Cull[_CullMode]
						Offset[_ZOffsetFactor],[_ZOffsetUnits]
						ColorMask[_ColorWriteMask]

						Stencil
						{
							Ref[_StencilReference]
							Comp[_StencilComparison]
							Pass[_StencilOperation]
						}

						CGPROGRAM

						#pragma vertex vert
						#pragma fragment frag
						#pragma geometry geom

						#pragma multi_compile_instancing
						#pragma multi_compile _ _HOVER_LIGHT_MEDIUM _HOVER_LIGHT_HIGH
						#pragma multi_compile _ _CLIPPING_PLANE _CLIPPING_SPHERE _CLIPPING_BOX

						#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON
						#pragma shader_feature _DISABLE_ALBEDO_MAP
						#pragma shader_feature _ _METALLIC_TEXTURE_ALBEDO_CHANNEL_A _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
						#pragma shader_feature _CHANNEL_MAP
						#pragma shader_feature _EMISSION
						#pragma shader_feature _TRIPLANAR_MAPPING
						#pragma shader_feature _LOCAL_SPACE_TRIPLANAR_MAPPING
						#pragma shader_feature _DIRECTIONAL_LIGHT
						#pragma shader_feature _SPECULAR_HIGHLIGHTS
						#pragma shader_feature _SPHERICAL_HARMONICS
						#pragma shader_feature _REFLECTIONS
						#pragma shader_feature _REFRACTION
						#pragma shader_feature _RIM_LIGHT
						#pragma shader_feature _VERTEX_COLORS
						#pragma shader_feature _VERTEX_EXTRUSION
						#pragma shader_feature _VERTEX_EXTRUSION_SMOOTH_NORMALS
						#pragma shader_feature _CLIPPING_BORDER
						#pragma shader_feature _NEAR_PLANE_FADE
						#pragma shader_feature _NEAR_LIGHT_FADE
						#pragma shader_feature _HOVER_LIGHT
						#pragma shader_feature _HOVER_COLOR_OVERRIDE
						#pragma shader_feature _PROXIMITY_LIGHT
						#pragma shader_feature _PROXIMITY_LIGHT_COLOR_OVERRIDE
						#pragma shader_feature _PROXIMITY_LIGHT_SUBTRACTIVE
						#pragma shader_feature _PROXIMITY_LIGHT_TWO_SIDED
						#pragma shader_feature _ROUND_CORNERS
						#pragma shader_feature _INDEPENDENT_CORNERS
						#pragma shader_feature _BORDER_LIGHT
						#pragma shader_feature _BORDER_LIGHT_USES_HOVER_COLOR
						#pragma shader_feature _BORDER_LIGHT_REPLACES_ALBEDO
						#pragma shader_feature _BORDER_LIGHT_OPAQUE
						#pragma shader_feature _INNER_GLOW
						#pragma shader_feature _IRIDESCENCE
						#pragma shader_feature _ENVIRONMENT_COLORING
						#pragma shader_feature _INSTANCED_COLOR
						#pragma shader_feature _IGNORE_Z_SCALE

						#define IF(a, b, c) lerp(b, c, step((fixed) (a), 0.0));

						#include "UnityCG.cginc"
						#include "UnityStandardConfig.cginc"
						#include "UnityStandardUtils.cginc"
						#include "/Assets/MRTK/StandardAssets/Shaders/MixedRealityShaderUtils.cginc"

					// This define will get commented in by the UpgradeShaderForLightweightRenderPipeline method.
					//#define _LIGHTWEIGHT_RENDER_PIPELINE

		#if defined(_TRIPLANAR_MAPPING) || defined(_DIRECTIONAL_LIGHT) || defined(_SPHERICAL_HARMONICS) || defined(_REFLECTIONS) || defined(_RIM_LIGHT) || defined(_PROXIMITY_LIGHT) || defined(_ENVIRONMENT_COLORING)
					#define _NORMAL
		#else
					#undef _NORMAL
		#endif

		#if defined(_CLIPPING_PLANE) || defined(_CLIPPING_SPHERE) || defined(_CLIPPING_BOX)
				#define _CLIPPING_PRIMITIVE
		#else
				#undef _CLIPPING_PRIMITIVE
		#endif

		#if defined(_NORMAL) || defined(_CLIPPING_PRIMITIVE) || defined(_NEAR_PLANE_FADE) || defined(_HOVER_LIGHT) || defined(_PROXIMITY_LIGHT)
					#define _WORLD_POSITION
		#else
					#undef _WORLD_POSITION
		#endif

		#if defined(_ALPHATEST_ON) || defined(_CLIPPING_PRIMITIVE) || defined(_ROUND_CORNERS)
					#define _ALPHA_CLIP
		#else
					#undef _ALPHA_CLIP
		#endif

		#if defined(_ALPHABLEND_ON)
					#define _TRANSPARENT
					#undef _ALPHA_CLIP
		#else
					#undef _TRANSPARENT
		#endif

		#if defined(_VERTEX_EXTRUSION) || defined(_ROUND_CORNERS) || defined(_BORDER_LIGHT)
					#define _SCALE
		#else
					#undef _SCALE
		#endif

		#if defined(_DIRECTIONAL_LIGHT) || defined(_RIM_LIGHT)
					#define _FRESNEL
		#else
					#undef _FRESNEL
		#endif

		#if defined(_ROUND_CORNERS) || defined(_BORDER_LIGHT) || defined(_INNER_GLOW)
					#define _DISTANCE_TO_EDGE
		#else
					#undef _DISTANCE_TO_EDGE
		#endif

					// HoloDataFlow shader must have UVs.
					#define _UV

					struct appdata_t
					{
						float4 vertex : POSITION;
						// The default UV channel used for texturing.
						float2 uv : TEXCOORD0;
						// Used for smooth normal data (or UGUI scaling data).
						float4 uv2 : TEXCOORD2;
						// Used for UGUI scaling data.
						float2 uv3 : TEXCOORD3;
		#if defined(_VERTEX_COLORS)
						fixed4 color : COLOR0;
		#endif
						fixed3 normal : NORMAL;
						fixed4 tangent : TANGENT;

						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct v2g
					{
						float4 position : SV_POSITION;
						float3 normal : NORMAL;
						float4 tangent : TANGENT;
		#if defined(_BORDER_LIGHT)
						float4 uv : TEXCOORD0;
		#elif defined(_UV)
						float2 uv : TEXCOORD0;
		#endif
		#if defined(_VERTEX_COLORS)
						fixed4 color : COLOR0;
		#endif
		#if defined(_SPHERICAL_HARMONICS)
						fixed3 ambient : COLOR1;
		#endif
		#if defined(_IRIDESCENCE)
						fixed3 iridescentColor : COLOR2;
		#endif
		#if defined(_WORLD_POSITION)
		#if defined(_NEAR_PLANE_FADE)
						float4 worldPosition : TEXCOORD2;
		#else
						float3 worldPosition : TEXCOORD2;
		#endif
		#endif
		#if defined(_SCALE)
						float3 scale : TEXCOORD3;
		#endif
		#if defined(_NORMAL)
		#if defined(_TRIPLANAR_MAPPING)
						fixed3 worldNormal : COLOR3;
						fixed3 triplanarNormal : COLOR4;
						float3 triplanarPosition : TEXCOORD6;
		#else
						fixed3 worldNormal : COLOR3;
		#endif
		#endif
						UNITY_VERTEX_OUTPUT_STEREO
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

					struct g2f
					{
						float4 position : SV_POSITION;
						float4 col : COLOR;
						float4 tan : TEXCOORD1;
		#if defined(_BORDER_LIGHT)
						float4 uv : TEXCOORD0;
		#elif defined(_UV)
						float2 uv : TEXCOORD0;
		#endif
		#if defined(_VERTEX_COLORS)
						fixed4 color : COLOR0;
		#endif
		#if defined(_SPHERICAL_HARMONICS)
						fixed3 ambient : COLOR1;
		#endif
		#if defined(_IRIDESCENCE)
						fixed3 iridescentColor : COLOR2;
		#endif
		#if defined(_WORLD_POSITION)
		#if defined(_NEAR_PLANE_FADE)
						float4 worldPosition : TEXCOORD2;
		#else
						float3 worldPosition : TEXCOORD2;
		#endif
		#endif
		#if defined(_SCALE)
						float3 scale : TEXCOORD3;
		#endif
		#if defined(_NORMAL)
		#if defined(_TRIPLANAR_MAPPING)
						fixed3 worldNormal : COLOR3;
						fixed3 triplanarNormal : COLOR4;
						float3 triplanarPosition : TEXCOORD6;
		#else
						fixed3 worldNormal : COLOR3;
		#endif
		#endif
						UNITY_VERTEX_OUTPUT_STEREO
						UNITY_VERTEX_INPUT_INSTANCE_ID
					};

		#if defined(_INSTANCED_COLOR)
					UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
					UNITY_INSTANCING_BUFFER_END(Props)
		#else
					fixed4 _Color;
		#endif
					sampler2D _MainTex;
					fixed4 _MainTex_ST;

		#if defined(_ALPHA_CLIP)
					fixed _Cutoff;
		#endif

					fixed _Metallic;
					fixed _Smoothness;

		#if defined(_CHANNEL_MAP)
					sampler2D _ChannelMap;
		#endif

		#if defined(_EMISSION)
					fixed4 _EmissiveColor;
		#endif

		#if defined(_TRIPLANAR_MAPPING)
					float _TriplanarMappingBlendSharpness;
		#endif

		#if defined(_DIRECTIONAL_LIGHT)
		#if defined(_LIGHTWEIGHT_RENDER_PIPELINE)
					CBUFFER_START(_LightBuffer)
					float4 _MainLightPosition;
					half4 _MainLightColor;
					CBUFFER_END
		#else
					fixed4 _LightColor0;
		#endif
		#endif

		#if defined(_REFRACTION)
					fixed _RefractiveIndex;
		#endif

		#if defined(_RIM_LIGHT)
					fixed3 _RimColor;
					fixed _RimPower;
		#endif

		#if defined(_VERTEX_EXTRUSION)
					float _VertexExtrusionValue;
		#endif

		#if defined(_CLIPPING_PLANE)
					fixed _ClipPlaneSide;
					float4 _ClipPlane;
		#endif

		#if defined(_CLIPPING_SPHERE)
					fixed _ClipSphereSide;
					float4 _ClipSphere;
		#endif

		#if defined(_CLIPPING_BOX)
					fixed _ClipBoxSide;
					float4 _ClipBoxSize;
					float4x4 _ClipBoxInverseTransform;
		#endif

		#if defined(_CLIPPING_PRIMITIVE)
					float _BlendedClippingWidth;
		#endif

		#if defined(_CLIPPING_BORDER)
					fixed _ClippingBorderWidth;
					fixed3 _ClippingBorderColor;
		#endif

		#if defined(_NEAR_PLANE_FADE)
					float _FadeBeginDistance;
					float _FadeCompleteDistance;
					fixed _FadeMinValue;
		#endif

		#if defined(_HOVER_LIGHT) || defined(_NEAR_LIGHT_FADE)
		#if defined(_HOVER_LIGHT_HIGH)
		#define HOVER_LIGHT_COUNT 10
		#elif defined(_HOVER_LIGHT_MEDIUM)
		#define HOVER_LIGHT_COUNT 4
		#else
		#define HOVER_LIGHT_COUNT 2
		#endif
		#define HOVER_LIGHT_DATA_SIZE 2
					float4 _HoverLightData[HOVER_LIGHT_COUNT * HOVER_LIGHT_DATA_SIZE];
		#if defined(_HOVER_COLOR_OVERRIDE)
					fixed3 _HoverColorOverride;
		#endif
		#endif

		#if defined(_PROXIMITY_LIGHT) || defined(_NEAR_LIGHT_FADE)
		#define PROXIMITY_LIGHT_COUNT 2
		#define PROXIMITY_LIGHT_DATA_SIZE 6
					float4 _ProximityLightData[PROXIMITY_LIGHT_COUNT * PROXIMITY_LIGHT_DATA_SIZE];
		#if defined(_PROXIMITY_LIGHT_COLOR_OVERRIDE)
					float4 _ProximityLightCenterColorOverride;
					float4 _ProximityLightMiddleColorOverride;
					float4 _ProximityLightOuterColorOverride;
		#endif
		#endif

		#if defined(_HOVER_LIGHT) || defined(_PROXIMITY_LIGHT) || defined(_BORDER_LIGHT)
					fixed _FluentLightIntensity;
		#endif

		#if defined(_ROUND_CORNERS)
		#if defined(_INDEPENDENT_CORNERS)
					float4 _RoundCornersRadius;
		#else
					fixed _RoundCornerRadius;
		#endif
					fixed _RoundCornerMargin;
		#endif

		#if defined(_BORDER_LIGHT)
					fixed _BorderWidth;
					fixed _BorderMinValue;
		#endif

		#if defined(_BORDER_LIGHT_OPAQUE)
					fixed _BorderLightOpaqueAlpha;
		#endif

		#if defined(_ROUND_CORNERS) || defined(_BORDER_LIGHT)
					fixed _EdgeSmoothingValue;
		#endif

		#if defined(_INNER_GLOW)
					fixed4 _InnerGlowColor;
					fixed _InnerGlowPower;
		#endif

		#if defined(_IRIDESCENCE)
					sampler2D _IridescentSpectrumMap;
					fixed _IridescenceIntensity;
					fixed _IridescenceThreshold;
					fixed _IridescenceAngle;
		#endif

		#if defined(_ENVIRONMENT_COLORING)
					fixed _EnvironmentColorThreshold;
					fixed _EnvironmentColorIntensity;
					fixed3 _EnvironmentColorX;
					fixed3 _EnvironmentColorY;
					fixed3 _EnvironmentColorZ;
		#endif

					// HoloDataFlow parameters
					sampler2D _ColorMap;
					float4 _ColorMap_ST;
					float _ScaleFactor;

		#if defined(_DIRECTIONAL_LIGHT)
					static const fixed _MinMetallicLightContribution = 0.7;
					static const fixed _IblContribution = 0.1;
		#endif

		#if defined(_SPECULAR_HIGHLIGHTS)
					static const float _Shininess = 800.0;
		#endif

		#if defined(_FRESNEL)
					static const float _FresnelPower = 8.0;
		#endif

		#if defined(_NEAR_LIGHT_FADE)
					static const float _MaxNearLightDistance = 10.0;

					inline float NearLightDistance(float4 light, float3 worldPosition)
					{
						return distance(worldPosition, light.xyz) + ((1.0 - light.w) * _MaxNearLightDistance);
					}
		#endif

		#if defined(_HOVER_LIGHT)
					inline float HoverLight(float4 hoverLight, float inverseRadius, float3 worldPosition)
					{
						return (1.0 - saturate(length(hoverLight.xyz - worldPosition) * inverseRadius)) * hoverLight.w;
					}
		#endif

		#if defined(_PROXIMITY_LIGHT)
					inline float ProximityLight(float4 proximityLight, float4 proximityLightParams, float4 proximityLightPulseParams, float3 worldPosition, float3 worldNormal, out fixed colorValue)
					{
						float proximityLightDistance = dot(proximityLight.xyz - worldPosition, worldNormal);
		#if defined(_PROXIMITY_LIGHT_TWO_SIDED)
						worldNormal = IF(proximityLightDistance < 0.0, -worldNormal, worldNormal);
						proximityLightDistance = abs(proximityLightDistance);
		#endif
						float normalizedProximityLightDistance = saturate(proximityLightDistance * proximityLightParams.y);
						float3 projectedProximityLight = proximityLight.xyz - (worldNormal * abs(proximityLightDistance));
						float projectedProximityLightDistance = length(projectedProximityLight - worldPosition);
						float attenuation = (1.0 - normalizedProximityLightDistance) * proximityLight.w;
						colorValue = saturate(projectedProximityLightDistance * proximityLightParams.z);
						float pulse = step(proximityLightPulseParams.x, projectedProximityLightDistance) * proximityLightPulseParams.y;

						return smoothstep(1.0, 0.0, projectedProximityLightDistance / (proximityLightParams.x * max(pow(normalizedProximityLightDistance, 0.25), proximityLightParams.w))) * pulse * attenuation;
					}

					inline fixed3 MixProximityLightColor(fixed4 centerColor, fixed4 middleColor, fixed4 outerColor, fixed t)
					{
						fixed3 color = lerp(centerColor.rgb, middleColor.rgb, smoothstep(centerColor.a, middleColor.a, t));
						return lerp(color, outerColor, smoothstep(middleColor.a, outerColor.a, t));
					}
		#endif

		#if defined(_ROUND_CORNERS)
					inline float PointVsRoundedBox(float2 position, float2 cornerCircleDistance, float cornerCircleRadius)
					{
						return length(max(abs(position) - cornerCircleDistance, 0.0)) - cornerCircleRadius;
					}

					inline fixed RoundCornersSmooth(float2 position, float2 cornerCircleDistance, float cornerCircleRadius)
					{
						return smoothstep(1.0, 0.0, PointVsRoundedBox(position, cornerCircleDistance, cornerCircleRadius) / _EdgeSmoothingValue);
					}

					inline fixed RoundCorners(float2 position, float2 cornerCircleDistance, float cornerCircleRadius)
					{
		#if defined(_TRANSPARENT)
						return RoundCornersSmooth(position, cornerCircleDistance, cornerCircleRadius);
		#else
						return (PointVsRoundedBox(position, cornerCircleDistance, cornerCircleRadius) < 0.0);
		#endif
					}
		#endif

		#if defined(_IRIDESCENCE)
					fixed3 Iridescence(float tangentDotIncident, sampler2D spectrumMap, float threshold, float2 uv, float angle, float intensity)
					{
						float k = tangentDotIncident * 0.5 + 0.5;
						float4 left = tex2D(spectrumMap, float2(lerp(0.0, 1.0 - threshold, k), 0.5), float2(0.0, 0.0), float2(0.0, 0.0));
						float4 right = tex2D(spectrumMap, float2(lerp(threshold, 1.0, k), 0.5), float2(0.0, 0.0), float2(0.0, 0.0));

						float2 XY = uv - float2(0.5, 0.5);
						float s = (cos(angle) * XY.x - sin(angle) * XY.y) / cos(angle);
						return (left.rgb + s * (right.rgb - left.rgb)) * intensity;
					}
		#endif

					v2g vert(appdata_t v)
					{
						v2g o;
						UNITY_SETUP_INSTANCE_ID(v);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

						float4 vertexPosition = v.vertex;

		#if defined(_WORLD_POSITION) || defined(_VERTEX_EXTRUSION)
						float3 worldVertexPosition = mul(unity_ObjectToWorld, vertexPosition).xyz;
		#endif

		#if defined(_SCALE)
						o.scale.x = length(mul(unity_ObjectToWorld, float4(1.0, 0.0, 0.0, 0.0)));
						o.scale.y = length(mul(unity_ObjectToWorld, float4(0.0, 1.0, 0.0, 0.0)));
		#if defined(_IGNORE_Z_SCALE)
						o.scale.z = o.scale.x;
		#else
						o.scale.z = length(mul(unity_ObjectToWorld, float4(0.0, 0.0, 1.0, 0.0)));
		#endif
		#if !defined(_VERTEX_EXTRUSION_SMOOTH_NORMALS)
						// uv3.y will contain a negative value when rendered by a UGUI and ScaleMeshEffect.
						if (v.uv3.y < 0.0)
						{
							o.scale.x *= v.uv2.x;
							o.scale.y *= v.uv2.y;
							o.scale.z *= v.uv3.x;
						}
		#endif
		#endif

						fixed3 localNormal = v.normal;

		#if defined(_NORMAL) || defined(_VERTEX_EXTRUSION)
						fixed3 worldNormal = UnityObjectToWorldNormal(localNormal);
		#endif

		#if defined(_VERTEX_EXTRUSION)
		#if defined(_VERTEX_EXTRUSION_SMOOTH_NORMALS)
						worldVertexPosition += UnityObjectToWorldNormal(v.uv2 * o.scale) * _VertexExtrusionValue;
		#else
						worldVertexPosition += worldNormal * _VertexExtrusionValue;
		#endif
						vertexPosition = mul(unity_WorldToObject, float4(worldVertexPosition, 1.0));
		#endif

						// HoloDataFlow makes the transformation in geometry shader, not vertex shader
						//o.position = UnityObjectToClipPos(vertexPosition);
						o.position = v.vertex;

						o.normal = v.normal;
						o.tangent = v.tangent;

		#if defined(_WORLD_POSITION)
						o.worldPosition.xyz = worldVertexPosition;
		#endif

		#if defined(_NEAR_PLANE_FADE)
						float rangeInverse = 1.0 / (_FadeBeginDistance - _FadeCompleteDistance);
		#if defined(_NEAR_LIGHT_FADE)
						float fadeDistance = _MaxNearLightDistance;

						[unroll]
						for (int hoverLightIndex = 0; hoverLightIndex < HOVER_LIGHT_COUNT; ++hoverLightIndex)
						{
							int dataIndex = hoverLightIndex * HOVER_LIGHT_DATA_SIZE;
							fadeDistance = min(fadeDistance, NearLightDistance(_HoverLightData[dataIndex], o.worldPosition));
						}

						[unroll]
						for (int proximityLightIndex = 0; proximityLightIndex < PROXIMITY_LIGHT_COUNT; ++proximityLightIndex)
						{
							int dataIndex = proximityLightIndex * PROXIMITY_LIGHT_DATA_SIZE;
							fadeDistance = min(fadeDistance, NearLightDistance(_ProximityLightData[dataIndex], o.worldPosition));
						}
		#else
						float fadeDistance = -UnityObjectToViewPos(vertexPosition).z;
		#endif
						o.worldPosition.w = max(saturate(mad(fadeDistance, rangeInverse, -_FadeCompleteDistance * rangeInverse)), _FadeMinValue);
		#endif

		#if defined(_BORDER_LIGHT) || defined(_ROUND_CORNERS)
						o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);

						float minScale = min(min(o.scale.x, o.scale.y), o.scale.z);

		#if defined(_BORDER_LIGHT)
						float maxScale = max(max(o.scale.x, o.scale.y), o.scale.z);
						float minOverMiddleScale = minScale / (o.scale.x + o.scale.y + o.scale.z - minScale - maxScale);

						float areaYZ = o.scale.y * o.scale.z;
						float areaXZ = o.scale.z * o.scale.x;
						float areaXY = o.scale.x * o.scale.y;

						float borderWidth = _BorderWidth;
		#endif

						if (abs(localNormal.x) == 1.0) // Y,Z plane.
						{
							o.scale.x = o.scale.z;
							o.scale.y = o.scale.y;

		#if defined(_BORDER_LIGHT)
							if (areaYZ > areaXZ&& areaYZ > areaXY)
							{
								borderWidth *= minOverMiddleScale;
							}
		#endif
						}
						else if (abs(localNormal.y) == 1.0) // X,Z plane.
						{
							o.scale.x = o.scale.x;
							o.scale.y = o.scale.z;

		#if defined(_BORDER_LIGHT)
							if (areaXZ > areaXY&& areaXZ > areaYZ)
							{
								borderWidth *= minOverMiddleScale;
							}
		#endif
						}
						else  // X,Y plane.
						{
							o.scale.x = o.scale.x;
							o.scale.y = o.scale.y;

		#if defined(_BORDER_LIGHT)
							if (areaXY > areaYZ&& areaXY > areaXZ)
							{
								borderWidth *= minOverMiddleScale;
							}
		#endif
						}

						o.scale.z = minScale;

		#if defined(_BORDER_LIGHT)
						float scaleRatio = min(o.scale.x, o.scale.y) / max(o.scale.x, o.scale.y);
						o.uv.z = IF(o.scale.x > o.scale.y, 1.0 - (borderWidth * scaleRatio), 1.0 - borderWidth);
						o.uv.w = IF(o.scale.x > o.scale.y, 1.0 - borderWidth, 1.0 - (borderWidth * scaleRatio));
		#endif
		#elif defined(_UV)
						o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		#endif

		#if defined(_VERTEX_COLORS)
						o.color = v.color;
		#endif

		#if defined(_SPHERICAL_HARMONICS)
						o.ambient = ShadeSH9(float4(worldNormal, 1.0));
		#endif

		#if defined(_IRIDESCENCE)
						float3 rightTangent = normalize(mul((float3x3)unity_ObjectToWorld, float3(1.0, 0.0, 0.0)));
						float3 incidentWithCenter = normalize(mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)) - _WorldSpaceCameraPos);
						float tangentDotIncident = dot(rightTangent, incidentWithCenter);
						o.iridescentColor = Iridescence(tangentDotIncident, _IridescentSpectrumMap, _IridescenceThreshold, v.uv, _IridescenceAngle, _IridescenceIntensity);
		#endif

		#if defined(_NORMAL)
		#if defined(_TRIPLANAR_MAPPING)
						o.worldNormal = worldNormal;
		#if defined(_LOCAL_SPACE_TRIPLANAR_MAPPING)
						o.triplanarNormal = localNormal;
						o.triplanarPosition = vertexPosition;
		#else
						o.triplanarNormal = worldNormal;
						o.triplanarPosition = o.worldPosition;
		#endif
		#else
						o.worldNormal = worldNormal;
		#endif
		#endif

						return o;
					}

					[maxvertexcount(2)]
					void geom(triangle v2g IN[3], inout LineStream<g2f> tristream)
					{
						g2f o;

						UNITY_SETUP_INSTANCE_ID(IN[0]);
						UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN[0]);

						if (length(IN[0].normal) == 0) {
							return;
						}

						// Support single-pass stereo rendering
						// https://docs.unity3d.com/Manual/SinglePassStereoRenderingHoloLens.html
						UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(IN[0], o);
						UNITY_TRANSFER_INSTANCE_ID(IN[0], o);

						float2 offset = -float2(0., _Time.z);
						fixed4 endColor = fixed4(1,1,1,1);
						fixed4 startColor = fixed4(.8, .8, .8, 1.);
						float _WeightFactor = _ScaleFactor * 0.45;

						/* We perform clipping in the geometry shader,
						   using the "main vertex" from IN[0].position as the position that
						   determines whether to clip.
						   This works sensibly for small _ScaleFactor values.

						   Why not do this in fragment shader (which would be more correct?):

						   - Because calling clip() or discard in fragment shader causes
							 a bug: instead of filled, the generated geometry is displayed
							 as wireframe.
							 Is it a bug of Unity, or Direct3D? Unknown.
					 This is possibly an outdated comment -- was relevant with old DataFlow shader,
					 not based on Mrtk2 standard shader.

						   - As a side effect, this is also faster. No need to calculate vertexWorld
							 for each generated vertex, no need to do clip in fragment shader.
						*/
		#if defined(_CLIPPING_PRIMITIVE)
						float primitiveDistance = 1.0;
		#if defined(_CLIPPING_PLANE)
						float4 mainVertexWorld = mul(unity_ObjectToWorld, IN[0].position);
						primitiveDistance = min(primitiveDistance, PointVsPlane(mainVertexWorld.xyz, _ClipPlane) * _ClipPlaneSide);
		#endif
						if (primitiveDistance < 0) {
							return;
						}
		#endif

						// Pass tangent to the fragment shader
						o.tan = IN[0].tangent;

						// Pass Mrtk2 standard stuff to fragment shader
		#if defined(_VERTEX_COLORS)
						o.color = IN[0].color;
		#endif
		#if defined(_SPHERICAL_HARMONICS)
						o.ambient = IN[0].ambient;
		#endif
		#if defined(_IRIDESCENCE)
						o.iridescentColor = IN[0].iridescentColor;
		#endif
		#if defined(_WORLD_POSITION)
		#if defined(_NEAR_PLANE_FADE)
						o.worldPosition = IN[0].worldPosition;
		#else
						o.worldPosition = IN[0].worldPosition;
		#endif
		#endif
		#if defined(_SCALE)
						o.scale = IN[0].scale;
		#endif
		#if defined(_NORMAL)
		#if defined(_TRIPLANAR_MAPPING)
						o.worldNormal = IN[0].worldNormal;
						o.triplanarNormal = IN[0].triplanarNormal;
						o.triplanarPosition = IN[0].triplanarPosition;
		#else
						o.worldNormal = IN[0].worldNormal;

						
		#endif
		#endif			
						float4 vertexObject;
						float4 move2 = float4(IN[0].normal * _ScaleFactor, 1);
						
						//-----
								vertexObject = IN[0].position;
								o.position = UnityObjectToClipPos(vertexObject); //1
								o.uv = float2(0.,0.) + offset;
								o.col = startColor;
								tristream.Append(o);

								vertexObject = IN[0].position + move2;
								o.position = UnityObjectToClipPos(vertexObject);
								o.uv = float2(.5,2.) + offset;
								o.col = endColor;
								tristream.Append(o);

								tristream.RestartStrip();

						}

							fixed4 frag(g2f i, fixed facing : VFACE) : SV_Target
							{
								UNITY_SETUP_INSTANCE_ID(i);
								UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);


								#define DISPLAY_MODE_1

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

					// Extracts information for lightmapping, GI (emission, albedo, ...)
					// This pass it not used during regular rendering.
					Pass
					{
						Name "Meta"
						Tags { "LightMode" = "Meta" }

						CGPROGRAM

						#pragma vertex vert
						#pragma fragment frag

						#pragma shader_feature EDITOR_VISUALIZATION
						#pragma shader_feature _EMISSION
						#pragma shader_feature _CHANNEL_MAP

						#include "UnityCG.cginc"
						#include "UnityMetaPass.cginc"

														// This define will get commented in by the UpgradeShaderForLightweightRenderPipeline method.
														//#define _LIGHTWEIGHT_RENDER_PIPELINE

														struct v2f
														{
															float4 vertex : SV_POSITION;
															float2 uv : TEXCOORD0;
														};

														float4 _MainTex_ST;

														v2f vert(appdata_full v)
														{
															v2f o;
															o.vertex = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
															o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);

															return o;
														}

														sampler2D _MainTex;
														sampler2D _ChannelMap;

														fixed4 _Color;
														fixed4 _EmissiveColor;

											#if defined(_LIGHTWEIGHT_RENDER_PIPELINE)
														CBUFFER_START(_LightBuffer)
														float4 _MainLightPosition;
														half4 _MainLightColor;
														CBUFFER_END
											#else
														fixed4 _LightColor0;
											#endif

														half4 frag(v2f i) : SV_Target
														{
															UnityMetaInput output;
															UNITY_INITIALIZE_OUTPUT(UnityMetaInput, output);

															output.Albedo = tex2D(_MainTex, i.uv) * _Color;
											#if defined(_EMISSION)
											#if defined(_CHANNEL_MAP)
															output.Emission += tex2D(_ChannelMap, i.uv).b * _EmissiveColor;
											#else
															output.Emission += _EmissiveColor;
											#endif
											#endif
											#if defined(_LIGHTWEIGHT_RENDER_PIPELINE)
															output.SpecularColor = _MainLightColor.rgb;
											#else
															output.SpecularColor = _LightColor0.rgb;
											#endif

															return UnityMetaFragment(output);
														}
														ENDCG
													}
				}

					Fallback "Hidden/InternalErrorShader"
															// Commented out for HoloDataFlow, it doesn't match our params now (we have some new, and we removed some old)
															// CustomEditor "Microsoft.MixedReality.Toolkit.Editor.MixedRealityStandardShaderGUI"
}
