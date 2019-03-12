using Unity;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;


public class STLSeriesConverter
{
    GameObject series_GameObject;


    [MenuItem("Holo/ImportSTL")]
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
        //TODO:  Exporting imported meshes to a prefab.
    }

}

public class STLImporter
{
    GameObject imported_STLs = new GameObject();
    List<string> file_paths = new List<string>();

    public STLImporter()
    {
        this.Get_file_paths();
    }


    // gets path to the subsequent STL meshes stored in a root folder.
    private void Get_file_paths()
    {
        string root_folder = EditorUtility.OpenFolderPanel("Select STL series root folder", "", "");
        string[] file
    }


    //loads separate files into one GameObject
    private void Load_files(string[] file_paths)
    {
        
        bool first_mesh = true;

    }

    public GameObject Get_GameObject()
    {
        return this.imported_STLs;
    }
}