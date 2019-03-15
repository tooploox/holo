using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//A class for importing a ASTL series from a dir
public class STLSeriesImporter
{
    GameObject importedSTLSeries = new GameObject("Hypertrophy");

    private string[] filePaths;

    public GameObject GetGameObject()
    {
        return importedSTLSeries;
    }

    // Object constructor, initiates STL series import.
    public STLSeriesImporter()
    {
        GetFilePaths();
        LoadFiles();
    }


    // gets path to the subsequent STL meshes stored in a root folder.
    private void GetFilePaths()
    {
        string rootFolder = EditorUtility.OpenFolderPanel("Select STL series root folder", Application.dataPath, "");
        filePaths = Directory.GetFiles(rootFolder + @"\");
    }


    //loads meshes from separate files into one GameObject
    private void LoadFiles()
    {
        SkinnedMeshRenderer skinnedMesh = importedSTLSeries.AddComponent<SkinnedMeshRenderer>();
        Mesh mesh = new Mesh();

        STLFileImporter stlFileImporter = new STLFileImporter();

        bool firstMesh = true;

        Debug.Log("Doing mesh #1!");
        stlFileImporter.LoadSTLFile(filePaths[0], firstMesh);
        mesh.vertices = stlFileImporter.BaseVertices;
        mesh.triangles = stlFileImporter.Indices;
        mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[0]), 100f, stlFileImporter.STLMeshVertices, null, null);
        skinnedMesh.sharedMaterial = Resources.Load<Material>("Test Object Material");
        firstMesh = false;

        int count = 2;
        for (int i = 1; i < filePaths.Length; i++)
        {
            Debug.Log("Doing mesh #" + count.ToString() + "!");

            stlFileImporter.LoadSTLFile(filePaths[i], firstMesh);

            // Check topology
            if (mesh.triangles.Length != stlFileImporter.Indices.Length)
                throw new Exception("Topology isn't the same");

            mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, stlFileImporter.STLMeshVertices, null, null);
            count++;
        }

        skinnedMesh.sharedMesh = mesh;
    }
}