using UnityEditor;
using UnityEngine;

using ModelLoad;

public class ModelLoader
{
    /* Loads a model in batchmode or multiple models in Editor and converts them into an AssetBundle.
     * To use in batchmode: "<Path to Unity.exe>" -quit -batchmode -logFile "<Path to the logfile>"  
    * -executeMethod ModelLoader.LoadVTKModel -rootDirectory "<Directory of the folder which stores the meshes>" 
    */

    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    [MenuItem("Holo/Convert edited VTK model to an AssetBundle's GameObject")]
    public static void LoadVTKModel()
    {
        SingleModel importedModel = new ImportedModel(false);
        LoadModel(importedModel);
    }

    [MenuItem("Holo/Convert GameObject model to an AssetBundle's GameObject")]
    public static void LoadGameObjectModel()
    {
        SingleModel importedModel = new GOModel();
        LoadModel(importedModel);
    }
    [MenuItem("Holo/Convert native VTK model to an AssetBundle's GameObject")]
    public static void LoadVTKWithConversion()
    {
        SingleModel importedModel = new ImportedModel(true);
        LoadModel(importedModel);
    }

    private static void LoadModel(SingleModel importedModel)
    {
        AssetBundleCreator assetBundleCreator = new AssetBundleCreator();

        bool loadModel = true;
        while (loadModel)
        {
            importedModel.GetModelData();
            assetBundleCreator.Create(importedModel);
            if (importedModel is ImportedModel)
            {
                ImportedModel model = (ImportedModel) importedModel;
                model.DeleteTmpData();
            }
            if (Application.isBatchMode)
            {
                loadModel = false;
            }
            else
            {
                loadModel = EditorUtility.DisplayDialog("", "Do you want to load another model?", "Yes", "No");
            }
        }
    }
}
