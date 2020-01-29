using UnityEditor;
using UnityEngine;

using ModelImport;

public class ModelLoader
{
    /* Loads a model in batchmode or multiple models in Editor and converts them into an AssetBundle.
     * To use in batchmode: "<Path to Unity.exe>" -quit -batchmode -logFile "<Path to the logfile>"  
    * -executeMethod ModelLoader.LoadVTKModel -rootDirectory "<Directory of the folder which stores the meshes>" 
    */

    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    //to be removed.
    [MenuItem("Holo/Convert edited VTK model to an AssetBundle's GameObject")]
    public static void LoadVTKModel()
    {
        ModelImport.ModelImporter importedModel = new ConvertedModel(false);
        LoadModel(importedModel);
    }

    [MenuItem("Holo/Convert GameObject model to an AssetBundle's GameObject")]
    public static void LoadGameObjectModel()
    {
        Log.Info("Started preprocessing!...");
        ModelImport.ModelImporter importedModel = new GOModel();
        LoadModel(importedModel);
    }
    [MenuItem("Holo/Convert native VTK model to an AssetBundle's GameObject")]
    public static void LoadVTKWithConversion()
    {
        Log.Info("Started preprocessing!...");
        ModelImport.ModelImporter importedModel = new ConvertedModel(true);
        LoadModel(importedModel);
    }

    private static void LoadModel(ModelImport.ModelImporter importedModel)
    {
        AssetBundleCreator assetBundleCreator = new AssetBundleCreator();

        bool loadModel = true;
        while (loadModel)
        {
            importedModel.GetModelData();
            assetBundleCreator.Create(importedModel);
            if (importedModel is ConvertedModel)
            {
                ConvertedModel model = (ConvertedModel) importedModel;
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
