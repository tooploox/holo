/* Shader for Holo model.

   Using Unity "shader variants" https://docs.unity3d.com/Manual/SL-MultipleProgramVariants.html
   to have optional clipping plane in the shader.

   Note that we cannot use surface shaders
   ( https://docs.unity3d.com/Manual/SL-SurfaceShaders.html ) for this.
   There are 2 approaches to two-sided lighting:

   - Separate logic for 2 passes (1st pass renders front faces,
     2nd pass renders back faces with inverted normals).
     This cannot work with surface shader, as surface shader generates multiple
     passes for us (trying to use "Pass" with surface shader will not compile).

     This is the approach we use now.

   - Conditionally invert normals in fragment shader (where we know whether
     the face is front or back facing).

     We don't use this approach now, but this may change in the future.
     TODO: try it.

     You cannot do this with surface shaders,
     since you can only revert normal in surface shader vertex function,
     but it's too soon,
     at this point you don't know whether it's front or back face.
*/

Shader "Holo/Model"
{
    // Show values to edit in inspector
    Properties
    {
        _Color("Diffuse", Color) = (0, 0, 0, 1)
        // Uncomment this and define MODEL_TEXTURED if necessary
        // _MainTex("Texture", 2D) = "white" {}
        _Emission("Emission", color) = (0, 0, 0, 1)
    }

    CGINCLUDE
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
            col += diffuseFactor * _Color * unity_LightColor[lightIndex];

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
    ENDCG

    SubShader
    {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            // LightMode Vertex defines proper unity_LightColor[] uniforms
            "LightMode" = "Vertex"
        }

        // Render front faces with default normal
        Pass {
            Name "Front Facing"

            Cull Back

            CGPROGRAM
            // See https://docs.unity3d.com/Manual/SL-MultipleProgramVariants.html
            // about Unity "shader variants".
            #pragma shader_feature CLIPPING_OFF CLIPPING_ON
            #pragma vertex vert
            #pragma fragment frag

            void flipNormal(inout float3 normal)
            {
                // Nothing needs to be done here
            }
            ENDCG
        }

        // Render back faces with inverted normal
        Pass {
            Name "Back Facing"

            Cull Front

            CGPROGRAM
            // See https://docs.unity3d.com/Manual/SL-MultipleProgramVariants.html
            // about Unity "shader variants".
            #pragma shader_feature CLIPPING_OFF CLIPPING_ON
            #pragma vertex vert
            #pragma fragment frag

            void flipNormal(inout float3 normal)
            {
                normal = -normal;
            }
            ENDCG
        }
    }

    FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}
