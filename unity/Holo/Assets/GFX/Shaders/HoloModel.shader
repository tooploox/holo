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

Shader "Holo/Model (Opaque)"
{
    // Show values to edit in inspector
    Properties
    {
        _Color("Diffuse", Color) = (0, 0, 0, 1)
        // Uncomment this and define MODEL_TEXTURED if necessary
        // _MainTex("Texture", 2D) = "white" {}
        _Emission("Emission", color) = (0, 0, 0, 1)
    }

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

            #include "HoloModelCommon.cginc"

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

            #include "HoloModelCommon.cginc"

            void flipNormal(inout float3 normal)
            {
                normal = -normal;
            }
            ENDCG
        }
    }

    FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}
