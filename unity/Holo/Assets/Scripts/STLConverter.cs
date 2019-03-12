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
        STLImporter stlImporter = new STLImporter();

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

public class STLImporter
{
    GameObject imported_STLs = new GameObject();
    List<string> file_paths = new List<string>();


    // Object constructor, initiates STL series import.
    public STLImporter()
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
        bool first_mesh = true;
        foreach (string filepath in this.file_paths)
        {
            this.Load_STL_file(filepath, first_mesh);
            first_mesh = false;
        }
    }
    
    //Loads a single STL file and turns it into a list of vertices (x,y,z) and if first_mesh: a list of indexes
    private void Load_STL_file(string file_path, bool first_mesh)
    {
        using (FileStream filestream = new FileStream(file_path, FileMode.Open, FileAccess.Read))
        {
            using (BinaryReader binary_reader = new BinaryReader(filestream, new ASCIIEncoding()))
            {
                // read header
                byte[] header = binary_reader.ReadBytes(80);
                uint facetCount = binary_reader.ReadUInt32();
                facets = new Facet[facetCount];

                for (uint i = 0; i < facetCount; i++)
                    facets[i] = binary_reader.GetFacet();
            }
        }

    }

    public GameObject Get_GameObject()
    {
        return this.imported_STLs;
    }
}