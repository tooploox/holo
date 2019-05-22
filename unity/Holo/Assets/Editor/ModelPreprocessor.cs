using System;
using UnityEditor;
using UnityEngine;

class ModelPreprocessor
{
    /* To use in batchmode: "<Path to Unity.exe>" -quit -batchmode -logFile "<Path to the logfile>" 
    * -executeMethod ModelPreprocessor.PreprocessModel -rootDirectory "<Directory of the folder which stores the meshes>"
    */
    [MenuItem("Holo/Convert model to a AssetBundle's GameObject")]
    public static void ConvertSeries()
    {
        ModelPreprocessor modelPreprocessor = new ModelPreprocessor();
        AssetBundleCreator assetBundleCreator = new AssetBundleCreator();

        string rootDirectory = modelPreprocessor.GetRootDirectory();
        (GameObject modelGameObject, Mesh modelMesh) = modelPreprocessor.LoadModelData(rootDirectory);
        assetBundleCreator.Create(modelGameObject, modelMesh);
    }

    // gets path to the root folder.
    private string GetRootDirectory()
    {
        string rootDirectory = "";
        if (Application.isBatchMode)
        {
            Debug.Log("It's Batchmode!");
            string[] args = Environment.GetCommandLineArgs();
            int directoryFlagIndex = Array.FindIndex(args, a => a.Equals("-rootDirectory"));
            Debug.Log("rootDirectoryIndex:" + directoryFlagIndex.ToString());
            rootDirectory = args[directoryFlagIndex + 1];
            if (String.IsNullOrEmpty(rootDirectory)) throw new Exception("Model's root directory has not been assigned!");
        }
        else
        {
            Debug.Log("It's not Batchmode!");
            rootDirectory = EditorUtility.OpenFolderPanel("Select STL series root folder", Application.dataPath, "");
        }
        if (String.IsNullOrEmpty(rootDirectory))
        {
            throw new ArgumentException("Path cannot be null!");
        }
        return rootDirectory;
    }

    private (GameObject, Mesh) LoadModelData(string rootDirectory)
    {
        FileSeriesImporter seriesImporter = new FileSeriesImporter(rootDirectory);
        return (seriesImporter.ModelGameObject, seriesImporter.ModelMesh);
    }
}
