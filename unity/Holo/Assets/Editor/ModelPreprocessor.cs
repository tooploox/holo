using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class ModelPreprocessor
{
    public string rootDirectory;

    private GameObject modelGameObject;
    private Mesh mesh;
    private AssetBundleCreator assetBundleCreator = new AssetBundleCreator();
    private List<string> assetFilePaths = new List<string>();

    /* To use in batchmode: "<Path to Unity.exe>" -quit -batchmode -logFile "<Path to the logfile>" 
     -executeMethod ModelPreprocessor.PreprocessModel -rootDirectory "<Directory of the folder which stores the meshes>"
    */
    [MenuItem("Holo/Convert model to a AssetBundle's GameObject")]
    public static void PreprocessModel()
    {
        ModelPreprocessor modelPreprocessor = new ModelPreprocessor();

        modelPreprocessor.GetRootDirectory();
        modelPreprocessor.GetModelData();
        modelPreprocessor.assetBundleCreator.Create(modelPreprocessor.modelGameObject, modelPreprocessor.mesh);
    }

    // gets path to the root folder.
    private void GetRootDirectory()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("It's Batchmode!");
            string[] args = Environment.GetCommandLineArgs();
            bool rootDirExists = false;
            for (int i = 0; i < args.Length; i++)
            {
                Debug.Log("ARG " + i + ": " + args[i]);
                if (args[i] == "-rootDirectory")
                {
                    rootDirectory = args[i + 1];
                    rootDirExists = true;
                }
            }
            if (!rootDirExists)
            {
                throw new Exception("Model's root directory has not been assigned!");
            }
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
    }

    private void GetModelData()
    {
        FileSeriesImporter seriesImporter = new FileSeriesImporter(rootDirectory);
        modelGameObject = seriesImporter.ModelGameObject;
        mesh = seriesImporter.ModelMesh;
    }


}
