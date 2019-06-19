using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using UnityEditor;

public class DataFlowVizualizer : MonoBehaviour
{
    private List<Vector3>[] points;
    private List<Vector3>[] vectors;
    private List<Vector3>[] endPoints;
    public string dirVtk;
    private int frameCount = 0;

    void Start()
    {
        if (Directory.Exists(dirVtk))
        {
            string[] fileEntries = Directory.GetFiles(dirVtk);
            frameCount = fileEntries.Length;

            points = new List<Vector3>[frameCount];
            vectors = new List<Vector3>[frameCount];
            endPoints = new List<Vector3>[frameCount];
            for (int i=0; i<frameCount; i++) {
                points[i] = new List<Vector3>();
                vectors[i] = new List<Vector3>();
                endPoints[i] = new List<Vector3>();
                ReadVTKData(i, fileEntries[i]);
                for (int j = 0; j < points[0].Count; j++)
                    endPoints[i].Add(points[i][j] + vectors[i][j]);
            }
        }

        CreateMesh();
    }

    private void CreateMesh()
    {
        int pointsCount = points[0].Count;
        Vector3[] vertices = new Vector3[pointsCount];
        Vector3[] vectorsAsNormals = new Vector3[pointsCount];
        for (int i = 0; i<vertices.Length; i++)
            vertices[i] = Vector3.zero;

        Mesh mesh = new Mesh();
        mesh.vertices = vertices; // baseVertices;
        mesh.normals = vertices;

        for (int frame = 0; frame < frameCount; frame++)
        {
            /*
            vertices = new Vector3[pointsCount * 3];
            Array.Copy(points[frame].ToArray(), vertices, pointsCount);
            Array.Copy(endPoints[frame].ToArray(), 0, vertices, pointsCount * 1, pointsCount);
            Array.Copy(points[frame].ToArray(), 0, vertices, pointsCount * 2, pointsCount);
            //put vector data inside normals data
            Vector3[] vectorsAsNormals = new Vector3[pointsCount * 3];
            Array.Copy(vectors[frame].ToArray(), vectorsAsNormals, pointsCount);
            Array.Copy(vectors[frame].ToArray(), 0, vectorsAsNormals, pointsCount * 1, pointsCount);
            Array.Copy(vectors[frame].ToArray(), 0, vectorsAsNormals, pointsCount * 2, pointsCount);
            */

            vertices = points[frame].ToArray();
            vectorsAsNormals = vectors[frame].ToArray();

            //add blendShape
            mesh.AddBlendShapeFrame(frame.ToString(), 100f, vertices, vectorsAsNormals, null); //last null is a tangentNormal, we can put there alpha and beta data
        }

        if (points[0].Count != endPoints[0].Count)
        {
            Debug.LogWarning("points[0].Count != vectors[0].Count " + points[0].Count + " " + endPoints[0].Count);
        }

        Vector2[] uvs = new Vector2[pointsCount * 3];
        int[] indices = new int[pointsCount * 3];

        for (int i = 0; i < indices.Length / 3; i++)
        {
            indices[3 * i] =     i; //start point
            indices[3 * i + 1] = i; //end point
            indices[3 * i + 2] = i; //start point
            uvs[0 * pointsCount + i] = new Vector2(0, 0);
            uvs[1 * pointsCount + i] = new Vector2(0, 0);
            uvs[2 * pointsCount + i] = new Vector2(0, 0);          
        }

        mesh.uv = uvs;

        // set bounds to middle and size of vertices1 (largest blend shape)
        mesh.bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(2f, 2f, 1f));
        mesh.SetIndices(indices,
            // From Unity docs ( https://docs.unity3d.com/ScriptReference/MeshTopology.LineStrip.html etc.) :
            // - Lines: Each two indices in the mesh index buffer form a line.
            // - LineStrip:  First two indices form a line, and then each new index connects a new vertex to the existing line strip.
            // - Points: In most of use cases, mesh index buffer should be "identity": 0, 1, 2, 3, 4, 5, ...
            MeshTopology.Triangles,
            // submesh
            0);

        // save Mesh to file
        AssetDatabase.DeleteAsset("Assets/test_faketris.mesh");
        AssetDatabase.CreateAsset(mesh, "Assets/test_faketris.mesh");

        GameObject go = new GameObject("Test Mesh SetTopology");
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        SkinnedMeshRenderer meshRenderer = go.AddComponent<SkinnedMeshRenderer>();
        meshRenderer.sharedMesh = mesh; // SkinnedMeshRenderer also needs to know the mesh
        //meshRenderer.sharedMaterial = ... we could assign here material
        go.AddComponent<BlendShapeAnimation>();

        // save GameObject to file
        AssetDatabase.DeleteAsset("Assets/test_faketris_game_object.prefab");
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/test_faketris_game_object.prefab");
    }

    void ReadVTKData(int frame, string fileVtk)
    {
        int readingStep = 0;
        StreamReader f = new StreamReader(fileVtk);
        while (!f.EndOfStream)
        {
            string l = f.ReadLine();

            /* reading steps:
             * 0 on first lines with string data
             * 1 when reading points
             * 2 in lines between points and vectors
             * 3 reading vectors
             * 
             * Important: In our new data structure this scenario doesn't work correctly, we need to read title lines of datasets
             * 
            */

            string[] split = l.Trim().Split(" ".ToCharArray());
            
            double x, y, z;
            if (split.Length == 3
                && double.TryParse(split[0], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out x)
                && double.TryParse(split[1], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out y)
                && double.TryParse(split[2], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out z))
            {
                Vector3 v = new Vector3((float)x, (float)y, (float)z);
                if (readingStep == 0) readingStep = 1;
                if (readingStep == 1) points[frame].Add(v);
                if (readingStep == 2) readingStep = 3;
                if (readingStep == 3) vectors[frame].Add(v);
            }
            else
            {
                if (readingStep == 1) readingStep = 2;
            }
        }
        f.Close();

        if (readingStep != 3)
            Debug.LogError(fileVtk + ": \r\n DataFlow read error: unsupported data structure in the vtk file");
        else if (points[frame].Count != vectors[frame].Count)
            Debug.LogError(fileVtk + "(" + points[frame].Count + ":" + vectors[frame].Count + "): \r\n DataFlow read error: points number not equal to vectors number");
        else
            Debug.Log(fileVtk + ": \r\n " + points[frame].Count + " points in Data Flow");
    }
    
}
