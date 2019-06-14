using UnityEditor;
using UnityEngine;

using ModelImport;

public class ModelLoader
{
    /* Loads a model in batchmode or multiple models in Editor and converts them into an AssetBundle.
     * To use in batchmode: "<Path to Unity.exe>" -quit -batchmode -logFile "<Path to the logfile>"  
    * -executeMethod ModelLoader.LoadModel -rootDirectory "<Directory of the folder which stores the meshes>" 
    */
    [MenuItem("Holo/Convert model to a AssetBundle's GameObject")]
    public static void LoadModel()
    {
        SingleModel importedModel = new SingleModel();
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
