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
    public string FileName { get; private set;}
    public GameObject GetGameObject()
    {
        return importedSTLSeries;
    }

    public Mesh GetMesh()
    {
        return mesh;
    }
    // Object constructor, initiates STL series import.
    public STLSeriesImporter()
    {
        GetFilePaths();
        GetFilename();
        LoadFiles();
    }


    // gets path to the subsequent STL meshes stored in a root folder.
    private void GetFilePaths()
    {
        string rootFolder = EditorUtility.OpenFolderPanel("Select STL series root folder", Application.dataPath, "");
        filePaths = Directory.GetFiles(rootFolder + @"\");
    }

    private void GetFilename()
    {
        string fileName = Path.GetFileNameWithoutExtension(filePaths[0]);

        while (true)
        {
            char lastChar = fileName[fileName.Length - 1];
            char endChar = '-';
            fileName = fileName.Remove(fileName.Length - 1);
            if (lastChar == endChar)
            {
                break;
            }
        }
        FileName = fileName;
        importedSTLSeries.name = fileName;
    }

    //loads meshes from separate files into one GameObject
    private void LoadFiles()
    {
        SkinnedMeshRenderer skinnedMesh = importedSTLSeries.AddComponent<SkinnedMeshRenderer>();
        STLFileImporter stlFileImporter = new STLFileImporter();

        //Configuring progress bar
        float progressChunk = (float) 1 / filePaths.Length;
        EditorUtility.DisplayProgressBar("Convert STL series to a .prefab", "Conversion in progress", 0);

        bool firstMesh = true;
        for (int i = 0; i < filePaths.Length; i++)
        {
            stlFileImporter.LoadSTLFile(filePaths[i], firstMesh);

            if(firstMesh)
            {
                mesh.vertices = stlFileImporter.BaseVertices;
                mesh.triangles = stlFileImporter.Indices;
                firstMesh = false;
            }

            // Check topology
            if (!mesh.triangles.SequenceEqual(stlFileImporter.Indices))
                Debug.LogWarning("Topology isn't the same!");

            mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, stlFileImporter.Vertices, stlFileImporter.Normals, null);

            EditorUtility.DisplayProgressBar("Convert STL series to a .prefab", "Conversion in progress", (i+1)*progressChunk);
        }

        skinnedMesh.sharedMesh = mesh;
        skinnedMesh.sharedMaterial = Resources.Load<Material>("Test Object Material");
        skinnedMesh.sharedMesh.RecalculateNormals();
        EditorUtility.ClearProgressBar();

    }
}