using UnityEngine;
using UnityEditor;

public class TestMeshTopology
{
    [MenuItem("EVPreprocessing/Test Mesh.SetTopology with lines")]
    static void Execute()
    {
        // base vertices are all zero
        Vector3[] baseVertices = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 0f),
        };
        // we will animate from vertices0 to vertices1
        Vector3[] vertices0 = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(1f, 1f, 0f),
            new Vector3(0f, 1f, 0f),
        };
        Vector3[] vertices1 = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(10f, 0f, 0f),
            new Vector3(10f, 10f, 0f),
            new Vector3(0f, 10f, 0f),
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices0; //  baseVertices;
        mesh.AddBlendShapeFrame("0", 100f, vertices0, null, null);
        mesh.AddBlendShapeFrame("1", 100f, vertices1, null, null);
        // set bounds to middle and size of vertices1 (largest blend shape)
        mesh.bounds = new Bounds(new Vector3(5f, 5f, 0f), new Vector3(10f, 10f, 1f));
        mesh.SetIndices(new int[]
            {
                0, 1,
                2, 3,
            },
            // From Unity docs ( https://docs.unity3d.com/ScriptReference/MeshTopology.LineStrip.html etc.) :
            // - Lines: Each two indices in the mesh index buffer form a line.
            // - LineStrip:  First two indices form a line, and then each new index connects a new vertex to the existing line strip.
            // - Points: In most of use cases, mesh index buffer should be "identity": 0, 1, 2, 3, 4, 5, ...
            MeshTopology.Lines,
            // submesh
            0);

        // save Mesh to file
        AssetDatabase.DeleteAsset("Assets/test_lines.mesh");
        AssetDatabase.CreateAsset(mesh, "Assets/test_lines.mesh");

        GameObject go = new GameObject("Test Mesh SetTopology");
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        SkinnedMeshRenderer meshRenderer = go.AddComponent<SkinnedMeshRenderer>();
        meshRenderer.sharedMesh = mesh; // SkinnedMeshRenderer also needs to know the mesh
        //meshRenderer.sharedMaterial = ... we could assign here material
        go.AddComponent<BlendShapeAnimation>();

        // save GameObject to file
        AssetDatabase.DeleteAsset("Assets/test_lines_game_object.prefab");
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/test_lines_game_object.prefab");
    }
}
