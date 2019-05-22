using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

//A class for importing a STL series from a directory.
public class FileSeriesImporter
{
    public GameObject ModelGameObject { get; private set; } = new GameObject();
    public Mesh ModelMesh { get; private set; } = new Mesh();

    private string[] filePaths;
    private string extension;

    private FileImporter fileImporter;
    private Dictionary<string, Vector3> boundingVertices = new Dictionary<string, Vector3>();
    
    // Object constructor, initiates file series import.
    public FileSeriesImporter(string rootDirectory)
    {
        GetFiles(rootDirectory);
        GetGameObjectName(rootDirectory);
        LoadFiles();
        ConfigureMesh();
    }

    //Loads file paths of particular frames and their extension
    private void GetFiles(string rootDirectory)
    {
        filePaths = Directory.GetFiles(rootDirectory + @"\");
        extension = Path.GetExtension(filePaths[0]);
    }

    //Gets name of the model basing on the folder containing frames
    private void GetGameObjectName(string rootFolder)
    {
        string rootdir = Path.GetFullPath(rootFolder).TrimEnd(Path.DirectorySeparatorChar);
        ModelGameObject.name = rootdir.Split(Path.DirectorySeparatorChar).Last();
    }

    //Loads meshes from separate files into Mesh Object as BlendShapeFrames
    private void LoadFiles()
    {
        fileImporter = new FileImporter(extension);
        ModelMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        //Configuring progress bar
        float progressChunk = (float) 1 / filePaths.Length;
        bool cancelImport = false;

        bool firstMesh = true;
        for(int i = 0; i < filePaths.Length; i++)
        {
            cancelImport = EditorUtility.DisplayCancelableProgressBar("Converting meshes", "Conversion in progress", (i + 1) * progressChunk);
            if (cancelImport)
            {
                AbortImport();
            }
            fileImporter.LoadFile(filePaths[i], firstMesh);

            if (firstMesh)
            {
                InitiateMesh();
            }
            CheckTopology(i);
            UpdateBounds(firstMesh);   
            ModelMesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, fileImporter.Vertices, fileImporter.Normals, null);
            firstMesh = false;
        }
    }

    //Function for aborting the import of a model
    private void AbortImport()
    {
        UnityEngine.Object.DestroyImmediate(ModelGameObject);
        EditorUtility.ClearProgressBar();
        throw new Exception("Convertion aborted");
    }

    // Mesh initiation.
    private void InitiateMesh()
    {
        ModelMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        if (fileImporter.IndicesInFacet == 2)
        {
            ModelMesh.vertices = fileImporter.BaseVertices;
            ModelMesh.SetIndices(fileImporter.Indices, MeshTopology.Lines, 0);
        }
        else
        {
            ModelMesh.vertices = fileImporter.BaseVertices;
            ModelMesh.triangles = fileImporter.Indices;
        }
    }

    //Checks if topology stays the same between current and first file, sending a warning if it doesn't.
    private void CheckTopology(int meshIndex)
    {
        bool equalTopology = ModelMesh.GetIndices(0).SequenceEqual(fileImporter.Indices);  
        if (!equalTopology)
        {
            Debug.LogWarning("Topology isn't the same! Mesh nr: " + meshIndex.ToString());
        }
    }

    //After each frame updates Boundingbox borders of the object.
    private void UpdateBounds(bool firstMesh)
    {
        if (firstMesh)
        {
            boundingVertices["minVertex"] = fileImporter.BoundingVertices["minVertex"];
            boundingVertices["maxVertex"] = fileImporter.BoundingVertices["maxVertex"];
        }
        else
        {
            boundingVertices["minVertex"] = Vector3.Min(boundingVertices["minVertex"], fileImporter.BoundingVertices["minVertex"]);
            boundingVertices["maxVertex"] = Vector3.Max(boundingVertices["maxVertex"], fileImporter.BoundingVertices["maxVertex"]);
        }
    }

    //Configures mesh into a BlendShape animation after loading all the frames.
    private void ConfigureMesh()
    {
        SkinnedMeshRenderer skinnedMesh = ModelGameObject.AddComponent<SkinnedMeshRenderer>();
        skinnedMesh.sharedMesh = ModelMesh;
        skinnedMesh.sharedMaterial = Resources.Load<Material>("MRTK_Standard_Gray");
        if (fileImporter.IndicesInFacet == 3)
        {
            skinnedMesh.sharedMesh.RecalculateNormals();
        }
        skinnedMesh.sharedMesh.bounds = CalculateBounds();
        EditorUtility.ClearProgressBar();
        ModelGameObject.AddComponent<BlendShapeAnimation>();
    }

    //Calculates Bounds for the GameObject after final extremities of the mesh series is known.
    private Bounds CalculateBounds()
    {
        Bounds meshBounds = new Bounds();
        Vector3 minVertex = boundingVertices["minVertex"];
        Vector3 maxVertex = boundingVertices["maxVertex"];
        meshBounds.center = (maxVertex + minVertex) / 2.0F;
        Vector3 extents = (maxVertex - minVertex) / 2.0F;
        for (int i = 0; i < 3; i++)
        {
            extents[i] = Math.Abs(extents[i]);
        }
        meshBounds.extents = extents;
        return meshBounds;
    }
}