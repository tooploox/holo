using UnityEditor;
using UnityEngine;

using ModelLoad;

public class ModelLoader
{
    /* Loads a model in batchmode or multiple models in Editor and converts them into an AssetBundle.
     * To use in batchmode: "<Path to Unity.exe>" -quit -batchmode -logFile "<Path to the logfile>"  
    * -executeMethod ModelLoader.LoadModel -rootDirectory "<Directory of the folder which stores the meshes>" 
    */
    [MenuItem("Holo/Convert VTK model to an AssetBundle's GameObject")]
    public static void LoadVTKModel()
    {
        SingleModel importedModel = new ImportedModel();
        LoadModel(importedModel);
    }

    [MenuItem("Holo/Convert GameObject model to an AssetBundle's GameObject")]
    public static void LoadGameObjectModel()
    {
        SingleModel importedModel = new GOModel();
        LoadModel(importedModel);
    }

    private static void LoadModel(SingleModel importedModel)
    {
	    AssetDirs.CreateDirectory(AssetDirs.TempAssetsDir);
	    AssetDirs.CreateDirectory(AssetDirs.TempAssetsResorcesDir);

        AssetBundleCreator assetBundleCreator = new AssetBundleCreator();

        bool loadModel = true;
        while (loadModel)
        {
            importedModel.GetModelData();
            assetBundleCreator.Create(importedModel);
            if (Application.isBatchMode)
            {
                loadModel = true;
            }
            else
            {
                loadModel = EditorUtility.DisplayDialog("", "Do you want to load another model?", "Yes", "No");
            }
        }
    }
}
