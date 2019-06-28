/* Copy of HoloModel.shader .
*/

Shader "Holo/Model (Transparent)"
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
            // "RenderType" = "Opaque"
            // "Queue" = "Geometry"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"

            // LightMode Vertex defines proper unity_LightColor[] uniforms
            "LightMode" = "Vertex"
        }

        // Render front faces with default normal
        Pass {
            Name "Front Facing"

            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

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
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

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
