using UnityEngine;
using UnityEditor;

public class TestBuildGameObjectWithBlendShapes
{
    [MenuItem("Holo/Test Build GameObject With BlendShapes")]
    private static void DoTest()
    {
        GameObject gameObject = new GameObject("Test With BlendShapes");
        SkinnedMeshRenderer skinnedMesh = gameObject.AddComponent<SkinnedMeshRenderer>();

        /* Define mesh triangles. 
         * Each triangle is defined as 3 indexes to the vertex array.
         * This example just defines a simple quad (2 triangles, connected by 1 edge).
         */
        int[] indexes = {
            0, 1, 2,
            0, 2, 3,
        };

        /* The blend shapes inform Unity how to interpolate between base vertex positions,
         * and all the blend shapes with non-zero weight.
         * In our case, in BlendShapeAnimation we always make sure that the blend shape weights
         * sum to 100, so the positions of baseVertices actually don't matter.
         */
        Vector3[] baseVertices = new Vector3[4];

        /* 3 possible mesh shapes. */
        Vector3[][] vertices = {
            new Vector3[] {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(1f, 1f, 0f),
                new Vector3(0f, 1f, 0f),
            },
            new Vector3[] {
                new Vector3(0f, 0f, 0f),
                new Vector3(0.1f, 0f, 0f),
                new Vector3(0.1f, 0.1f, 0f),
                new Vector3(0f, 0.1f, 0f),
            },
            new Vector3[] {
                new Vector3(10f, 10f, 0f),
                new Vector3(11f, 10f, 0f),
                new Vector3(11f, 11f, 0f),
                new Vector3(10f, 11f, 0f),
            }
        };

        Mesh mesh = new Mesh();
        mesh.vertices = baseVertices;
        mesh.triangles = indexes;

        /* For each blend shape, call AddBlendShapeFrame.
         * See https://docs.unity3d.com/ScriptReference/Mesh.AddBlendShapeFrame.html .
         * The frameWeight (2nd parameter) should always be 100f -- internally, blend shape effect may be defined
         * by multiple "frames", but this is not useful for us.
         */
        mesh.AddBlendShapeFrame("vertices0", 100f, vertices[0], null, null);
        mesh.AddBlendShapeFrame("vertices1", 100f, vertices[1], null, null);
        mesh.AddBlendShapeFrame("vertices2", 100f, vertices[2], null, null);

        skinnedMesh.sharedMesh = mesh;
        skinnedMesh.sharedMaterial = Resources.Load<Material>("Test Object Material");

        // for ease of testing, add BlendShapeAnimation to it
        gameObject.AddComponent<BlendShapeAnimation>();
    }
}
