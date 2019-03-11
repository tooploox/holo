using Unity;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;


public class STLConverter
{
    [MenuItem("Holo/ImportSTL")]
    public static void ConvertSTL()
    {
        STLConverter stlconverter = new STLConverter();

        GameObject imported_Mesh = stlconverter.Import_stl();
        imported_Mesh.AddComponent<BlendShapeAnimation>();
        stlconverter.ExportToPrefab(imported_Mesh);
    }

    //Imports STL series and turns it into a game object
    private GameObject Import_stl()
    {
        string[] file_paths = this.Get_file_paths();
        GameObject imported_Mesh = this.load_files(file_paths);

        return imported_Mesh;
    }

    // gets path to the subsequent STL meshes stored in a root folder.
    private string[] Get_file_paths()
    {
        string[] path_array = { 'Foo' };
        // TODO: Getting the actual path
        return path_array
    }


    //loads separate files into one GameObject
    private GameObject load_files(string[] file_paths)
    {



        return
    }


    // Exports finished GameObject to a .prefab
    private void ExportToPrefab(GameObject Mesh)
    {
        //TODO:  Exporting imported meshes to a prefab.
    }

}
