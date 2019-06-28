// Shared shader code,
// for both front and back faces rendering (used in both passes).

#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc"

// MODEL_TEXTURED is not defined for now, but in the future we may want to render textured model
//#define MODEL_TEXTURED

// In case of Phong shading, fragment shader calculates lighting.
// This looks better, esp. specular highlights, if we implement them.
// Otherwise, vertex shader calculates lighting (faster).
//#define PHONG_SHADING

// Forward declaration, will be implemented differently in each pass
void flipNormal(inout float3 normal);

#ifdef CLIPPING_ON
float4 _Plane;
#endif

fixed4 _Color;
fixed4 _Emission;

/* Calculate lighting using vertex and normal (normalized) in eye space. */
fixed4 lighting(float3 vertexEye, float3 normalEye)
{
    // start with _Emission (per material) + UNITY_LIGHTMODEL_AMBIENT (global),
    // and alpha from _Color
    fixed4 col = fixed4(_Emission.xyz + UNITY_LIGHTMODEL_AMBIENT.xyz, _Color.w);

    // adjust this to Holo scene content.
    // Careful with moving this line around, e.g. placing it outside of frag()
    // makes the "for" loop below not executing.
    const int lightsCount = 2;

    /* Calculate simple lighting with diffuse and specular.
       We don't use ShadeVertexLightsFull as it doesn't account
       for point and dir lights, and doesn't make specular. */
    for (int lightIndex = 0; lightIndex < lightsCount; lightIndex++)
    {
        float3 directionToLight = unity_LightPosition[lightIndex].w != 0 ?
          // point light
          normalize(unity_LightPosition[lightIndex].xyz - vertexEye) :
          // directional light
          unity_LightPosition[lightIndex].xyz;
        half diffuseFactor = max(0, dot(normalEye, directionToLight));
        col.rgb += diffuseFactor * _Color.rgb * unity_LightColor[lightIndex].rgb;

        // More light features can be implemented here as needed:
        // - specular (consider calculating it in fragment shader then for best look)
        // - attenuation
    }

    return col;
}

struct v2f
{
    float4 vertexClip : SV_POSITION;

    #ifdef MODEL_TEXTURED
    float2 uv : TEXCOORD0;
    #endif

    // Used by clip plane in fragment shader
    float4 vertexWorld : TEXCOORD1;

    #ifdef PHONG_SHADING
    float3 vertexEye : TEXCOORD2;
    half3 normalEye : TEXCOORD3;
    #else
    fixed4 lighting : TEXCOORD2;
    #endif
};

v2f vert (appdata_base v)
{
    v2f o;

    // calculate vertex stuff
    o.vertexClip = UnityObjectToClipPos(v.vertex);
    float3 vertexEye = UnityObjectToViewPos(v.vertex);
    o.vertexWorld = mul(unity_ObjectToWorld, v.vertex);

    // calculate texture stuff
    #ifdef MODEL_TEXTURED
    o.uv = v.texcoord;
    #endif

    // calculate normal stuff
    // flip normal, depending on whether we render front or back faces
    flipNormal(v.normal);
    //o.normalWorld = UnityObjectToWorldNormal(v.normal);
    // to be correct (correct interpolation, even when scaling),
    // both vertex and fragment should normalize() the normal vector
    float3 normalEye = normalize(mul(UNITY_MATRIX_IT_MV, v.normal).xyz);

    // calculate lighting, or pass parameters to calculate it in fragment shader
    #ifdef PHONG_SHADING
    o.vertexEye = vertexEye;
    o.normalEye = normalEye;
    #else
    o.lighting = lighting(vertexEye, normalEye);
    #endif

    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    #ifdef CLIPPING_ON
    //calculate signed distance to plane
    float distance = dot(i.vertexWorld, _Plane.xyz);
    distance = distance + _Plane.w;
    // discard surface above plane
    clip(-distance);
    #endif

    #ifdef PHONG_SHADING
    fixed4 col = lighting(i.vertexEye, normalize(i.normalEye));
    #else
    fixed4 col = i.lighting;
    #endif

    #ifdef MODEL_TEXTURED
    col *= tex2D(_MainTex, i.uv);
    #endif
    return col;
}
