using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//A class for importing a STL series from a dir
public class FileSeriesImporter
{
    private GameObject seriesGameObject = new GameObject();
    private Mesh mesh = new Mesh();

    private string[] filePaths;
    private string extension;

    private bool cancelConvertion = false;
    private bool equalTopology = false; 

    public GameObject GetGameObject()
    {
        return seriesGameObject;
    }

    public Mesh GetMesh()
    {
        return mesh;
    }
    
    // Object constructor, initiates file series import.
    public FileSeriesImporter(string rootDirectory)
    {
        GetFiles(rootDirectory);
        GetGameObjectName(rootDirectory);
        LoadFiles();
    }

    private void GetFiles(string rootDirectory)
    {
        filePaths = Directory.GetFiles(rootDirectory + @"\");
        extension = Path.GetExtension(filePaths[0]);
    }
    private void GetGameObjectName(string rootFolder)
    {
        string rootdir = Path.GetFullPath(rootFolder).TrimEnd(Path.DirectorySeparatorChar);
        seriesGameObject.name = rootdir.Split(Path.DirectorySeparatorChar).Last();
    }

    //loads meshes from separate files into one GameObject
    private void LoadFiles()
    {
        SkinnedMeshRenderer skinnedMesh = seriesGameObject.AddComponent<SkinnedMeshRenderer>();
        FileImporter fileImporter = new FileImporter(extension);
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        //Configuring progress bar
        float progressChunk = (float) 1 / filePaths.Length;
        cancelConvertion = EditorUtility.DisplayCancelableProgressBar("Converting meshes", "Conversion in progress", 0);

        bool firstMesh = true;
        for(int i = 0; i < filePaths.Length; i++)
        {
            if (cancelConvertion)
            {
                UnityEngine.Object.DestroyImmediate(seriesGameObject);
                EditorUtility.ClearProgressBar();
                throw new Exception("Convertion aborted");
            }
            fileImporter.LoadFile(filePaths[i], firstMesh);

            if(firstMesh)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.vertices = fileImporter.BaseVertices;
                mesh.triangles = fileImporter.Indices;
                firstMesh = false;
            }

            // Check topology
            equalTopology = mesh.triangles.SequenceEqual(fileImporter.Indices);
            if (!equalTopology)
            {
                Debug.LogWarning("Topology isn't the same! Mesh nr: " + i.ToString());
                for (int index = 0; index < fileImporter.Indices.Length; index++)
                {
                    if (fileImporter.Indices[index] != mesh.triangles[index])
                        Debug.Log("fileimporter vertex nr" + index.ToString() + ": " + fileImporter.Indices[index] + " \n" +
                            "meshtriangles vertex" + index.ToString() + ": " + mesh.triangles[index] + " \n");
                }
            }
            mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, fileImporter.Vertices, fileImporter.Normals, null);

            cancelConvertion = EditorUtility.DisplayCancelableProgressBar("Converting meshes", "Conversion in progress", (i+1)*progressChunk);
        }
        skinnedMesh.sharedMesh = mesh;
        skinnedMesh.sharedMaterial = Resources.Load<Material>("MRTK_Standard_Gray");
        skinnedMesh.sharedMesh.RecalculateNormals();
        skinnedMesh.sharedMesh.RecalculateBounds();
        //skinnedMesh.sharedMesh.bounds = CalculateBounds(fileImporter.BoundingVertices);
        EditorUtility.ClearProgressBar();
        seriesGameObject.AddComponent<BlendShapeAnimation>();
    }

    private Bounds CalculateBounds(Dictionary<string, Vector3> boundingVertices)
    {
        Bounds meshBounds = new Bounds();
        Vector3 minVertex = boundingVertices["minVertex"];
        Vector3 maxVertex = boundingVertices["maxVertex"];
        
        Vector3 extents = (maxVertex - minVertex) / 2.0F;
        for (int i = 0; i < 3; i++)
            if (meshBounds.extents[i] < 0)
                extents[i] = -extents[i];

        meshBounds.center = (maxVertex + minVertex) / 2.0F;
        meshBounds.extents = extents;
        return meshBounds;
    }
}