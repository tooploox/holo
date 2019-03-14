using Unity;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.IO;

/// A class for importing and converting StL series into an animation
public class STLSeriesConverter
{
    GameObject seriesGameObject;
    
    [MenuItem("Holo/Convert STL series to a .prefab")]
    public static void ConvertSTL()
    {
        STLSeriesConverter stlSeriesConverter = new STLSeriesConverter();
        STLSeriesImporter stlImporter = new STLSeriesImporter();

        stlSeriesConverter.seriesGameObject = stlImporter.GetGameObject();
        stlSeriesConverter.seriesGameObject.AddComponent<BlendShapeAnimation>();

        stlSeriesConverter.ExportToPrefab();
    }

    // Exports finished GameObject to a .prefab
    private void ExportToPrefab()
    {
        string save_path = EditorUtility.SaveFilePanel("Export to a  .prefab file", Application.dataPath, "", ".prefab");
        PrefabUtility.SaveAsPrefabAsset(seriesGameObject, save_path);
    }

}

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
        mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[0]), 100f, stlFileImporter.AllVertices, null, null);
        skinnedMesh.sharedMaterial = Resources.Load<Material>("Test Object Material");
        firstMesh = false;

        int count = 2;
        for (int i = 1; i < filePaths.Length; i++)
        {
            Debug.Log("Doing mesh #" + count.ToString() + "!");

            stlFileImporter.LoadSTLFile(filePaths[i], firstMesh);

            // Check topology
            if (!mesh.triangles.SequenceEqual(stlFileImporter.Indices))
                throw new Exception("Topology isn't the same");

            mesh.AddBlendShapeFrame(Path.GetFileName(filePaths[i]), 100f, stlFileImporter.AllVertices, null, null);
            count++;
        }

        skinnedMesh.sharedMesh = mesh;
    }
}


//Loads a single STL file and turns it into a list of vertices (x,y,z) & if firstMesh: a list of indexes
public class STLFileImporter
{
    public Vector3[] BaseVertices { get; private set; }

    private List<Vector3> allVertices = new List<Vector3>();
    public Vector3[] AllVertices { get => allVertices.ToArray(); }

    private List<int> indices = new List<int>();
    public int[] Indices { get => indices.ToArray(); }

    private uint facetCount = 1;

    public void LoadSTLFile(string file_path, bool firstMesh)
    {
        allVertices = new List<Vector3>();
        indices = new List<int>();
        using (FileStream filestream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader binaryReader = new BinaryReader(filestream, new ASCIIEncoding()))
            {
                // read header
                byte[] header = binaryReader.ReadBytes(80);
                facetCount = binaryReader.ReadUInt32();

                for (uint i = 0; i < facetCount; i++)
                    AdaptFacet(binaryReader, firstMesh);
            }
        }
        if (firstMesh)
            BaseVertices = new Vector3[allVertices.Count];
    }

    private void AdaptFacet(BinaryReader binaryReader, bool firstMesh)
    {
        binaryReader.GetVector3(); // A normal we don't use

        for (int i = 0; i < 3; i++)
        {
            Vector3 vertex = binaryReader.GetVector3();
            AddVertex(vertex);
        }
        binaryReader.ReadUInt16(); // non-sense attribute byte
    }

    private void AddVertex(Vector3 vertex)
    {
        AddIndex(vertex);
        allVertices.Add(vertex);
    }

    private void AddIndex(Vector3 currentVertex)
    {
        int index = 0;
        foreach (Vector3 listedVertex in allVertices)
        {
            if (listedVertex.Equals(currentVertex))
            {
                indices.Add(index);
                return;
            }
            index++;
        }
        indices.Add(index);
    }
}

//Static class containing methods for extracting and adapting vertices from a STL file. 
public static class STLImportUtils
{
    public static Vector3 GetVector3(this BinaryReader binaryReader)
    {
        Vector3 vector3 = new Vector3();
        for (int i = 0; i < 3; i++)
        {
            vector3[i] = binaryReader.ReadSingle();
        }
        
        //maintaining Unity counter-clockwise orientation
        vector3.z = -vector3.z;

        return vector3;
    }
}