using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//A class for importing a STL series from a dir
public class STLSeriesImporter
{
    GameObject importedSTLSeries = new GameObject();
    Mesh mesh = new Mesh();
    private string[] filePaths;

    bool cancelConvertion = false;
    bool equalTopology = false; 

    public GameObject GetGameObject()
    {
        return importedSTLSeries;
    }

    public Mesh GetMesh()
    {
        return mesh;
    }
    // Object constructor, initiates STL series import.
    public STLSeriesImporter(string rootFolder)
    {
        filePaths = Directory.GetFiles(rootFolder + @"\");
        GetFilename();
        LoadFiles();
    }




    private void GetFilename()
    {
        string fileName = Path.GetFileNameWithoutExtension(filePaths[0]);
        int lastCharIndex = fileName.LastIndexOf("-");
        if (lastCharIndex == -1)
            importedSTLSeries.name = fileName;
        else
            importedSTLSeries.name = fileName.Substring(0, lastCharIndex);

    }

    //loads meshes from separate files into one GameObject
    private void LoadFiles()
    {
        SkinnedMeshRenderer skinnedMesh = importedSTLSeries.AddComponent<SkinnedMeshRenderer>();
        STLFileImporter stlFileImporter = new STLFileImporter();

        //Configuring progress bar
        float progressChunk = (float) 1 / filePaths.Length;
        cancelConvertion = EditorUtility.DisplayCancelableProgressBar("Convert STL series to a .prefab", "Conversion in progress", 0);

        bool firstMesh = true;
        for (int i = 0; i < filePaths.Length; i++)
        {
            if (cancelConvertion)
            {
                UnityEngine.Object.DestroyImmediate(importedSTLSeries);
                EditorUtility.ClearProgressBar();
                throw new Exception("Convertion aborted");
            }
            stlFileImporter.LoadSTLFile(filePaths[i], firstMesh);

            if(firstMesh)
            {
                mesh.vertices = stlFileImporter.BaseVertices;
                mesh.triangles = stlFileImporter.Indices;
                firstMesh = false;
            }

            // Check topology
            equalTopology = mesh.triangles.SequenceEqual(stlFileImporter.Indices);
            if (!equalTopology)
                Debug.LogWarning("Topology isn't the same!");

            mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, stlFileImporter.Vertices, stlFileImporter.Normals, null);

            cancelConvertion = EditorUtility.DisplayCancelableProgressBar("Convert STL series to a .prefab", "Conversion in progress", (i+1)*progressChunk);

        }

        skinnedMesh.sharedMesh = mesh;
        skinnedMesh.sharedMaterial = Resources.Load<Material>("Test Object Material");
        skinnedMesh.sharedMesh.RecalculateNormals();
        EditorUtility.ClearProgressBar();

    }
}