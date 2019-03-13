using Unity;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using System.IO;


public class STLSeriesConverter
{
    GameObject series_GameObject;
    
    [MenuItem("Holo/Convert STL series to a .prefab")]
    public static void ConvertSTL()
    {
        STLSeriesConverter stlSeriesConverter = new STLSeriesConverter();
        STLSeriesImporter stlImporter = new STLSeriesImporter();

        stlSeriesConverter.series_GameObject = stlImporter.Get_GameObject();
        stlSeriesConverter.series_GameObject.AddComponent<BlendShapeAnimation>();

        stlSeriesConverter.ExportToPrefab();
    }

    // Exports finished GameObject to a .prefab
    private void ExportToPrefab()
    {
        string save_path = EditorUtility.SaveFilePanel("Choose a folder where for a .prefab file", "", "", ".prefab");
        PrefabUtility.SaveAsPrefabAsset(series_GameObject, save_path);
    }

}

public class STLSeriesImporter
{
    GameObject imported_STLs = new GameObject();
    
    List<string> file_paths = new List<string>();

    public GameObject Get_GameObject()
    {
        return this.imported_STLs;
    }

    // Object constructor, initiates STL series import.
    public STLSeriesImporter()
    {
        this.Get_file_paths();
        this.Load_files();
    }


    // gets path to the subsequent STL meshes stored in a root folder.
    private void Get_file_paths()
    {
        string root_folder = EditorUtility.OpenFolderPanel("Select STL series root folder", "", "");
        string[] filenames = Directory.GetFiles(root_folder);
    }


    //loads meshes from separate files into one GameObject
    private void Load_files()
    {
        SkinnedMeshRenderer skinnedMesh = imported_STLs.AddComponent<SkinnedMeshRenderer>();
        Mesh mesh = new Mesh();

        STLFileImporter stlFileImporter = new STLFileImporter();

        bool first_mesh = true;
        foreach (string filepath in this.file_paths)
        {
            stlFileImporter.Load_STL_file(filepath, first_mesh);
            mesh.AddBlendShapeFrame(Path.GetFileName(filepath), 100f, stlFileImporter.AllVertices, null, null);
            first_mesh = false;
        }
        mesh.vertices = stlFileImporter.BaseVertices;
        mesh.triangles = stlFileImporter.Indices;
        skinnedMesh.sharedMesh = mesh;
    }
}


//Loads a single STL file and turns it into a list of vertices (x,y,z) & if first_mesh: a list of indexes
public class STLFileImporter
{
    public Vector3[] BaseVertices { get; private set; }

    private List<Vector3> allVertices;
    public Vector3[] AllVertices { get => allVertices.ToArray(); }

    private List<int> indices;
    public int[] Indices { get => indices.ToArray(); }


    // TODO: Change arrays into list but getting them gives you an array

    public void Load_STL_file(string file_path, bool first_mesh)
    {
        using (FileStream filestream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader binary_reader = new BinaryReader(filestream, new ASCIIEncoding()))
            {
                // read header
                byte[] header = binary_reader.ReadBytes(80);
                uint facetCount = binary_reader.ReadUInt32();

                if (first_mesh)
                {
                    BaseVertices = new Vector3[facetCount*3];
                }
                
                //TODO check topology
                 

                for (uint i = 0; i < facetCount; i++)
                {
                    Adapt_Facet(binary_reader, first_mesh);
                }
            }
        }
    }

    private void Adapt_Facet(BinaryReader binary_reader, bool first_mesh)
    {
        binary_reader.GetVector3(); // A normal we don't use

        for (int i = 0; i < 3; i++)
        {
            Vector3 vertix = binary_reader.GetVector3();
            if (first_mesh)
            {
                SetIndexForVertix(vertix);
            }
            allVertices.Add(vertix);
        }
        binary_reader.ReadUInt16(); // non-sense attribute byte
    }

    private void SetIndexForVertix(Vector3 vertix)
    {
        int index = 0;
        foreach (Vector3 listed_vertix in allVertices)
        {
            if (listed_vertix.Equals(vertix))
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
            if (i == 2)
                vector3[i] = -vector3[i];
        }
        return vector3;
    }
}