using System.IO;
using UnityEditor;
using UnityEngine;

using ModelImport;

public class DataPreparator
{
    /* Loads a model in batchmode or multiple models in Editor and converts them into an AssetBundle.
     * To use in batchmode: "<Path to Unity.exe>" -quit -batchmode -logFile "<Path to the Unitys logfile>"  -projectPath "<path to the EVPreprocessing project>"
    * -executeMethod DataPreparator.ImportWithConversion --RootDirectory "<Directory of the folder which stores the meshes>" --OutputDir ""<Directory where ABs will be stored" --LogDir "<directory where debug logfiles will be stored>" 
    */

    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    [MenuItem("EVPreprocessing/Create an AssetBundle from an external supported format")]
    public static void ImportWithConversion()
    {
        var importDispatcher = new DataPreparator();
        importDispatcher.PrepareData("ConversionRequired");
    }

    [MenuItem("EVPreprocessing/Create an AssetBundle from volumetric data")]
    public static void ImportVolumetric()
    {
        var importDispatcher = new DataPreparator();
        importDispatcher.PrepareData("VolumetricModel");
    }

    [MenuItem("EVPreprocessing/Create an AssetBundle from converted data")]
    public static void ImportConvertedModel()
    {
        var importDispatcher = new DataPreparator();
        importDispatcher.PrepareData("ConvertedModel");
    }

    [MenuItem("EVPreprocessing/Create an AssetBundle from a Unity-supported format")]
    public static void ImportGameObjectModel()
    {
        var importDispatcher = new DataPreparator();
        importDispatcher.PrepareData("UnityNative");
    }


    private void PrepareData(string modelType)
    { 
        var inputConfig = new InputConfiguration();
        AssetDirs.CreateAssetDirectory(AssetDirs.TempAssetsDir);
        var assetBundleCreator = new AssetBundleCreator(inputConfig.OutputDir);
        var modelConverter = new ModelConverter();

        Log.Info("Preprocessing started!");
        var modelImporter = InitializeModelImporter(modelType, modelConverter, inputConfig.RootDirectory);
        bool loadModel = true;
        try
        {
            while (loadModel)
            {
                modelImporter.GetModelData();
                assetBundleCreator.Create(modelImporter);
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
        finally
        {
            RecursiveDeleter.DeleteRecursivelyWithSleep(AssetDirs.TempAssetsDir);
            Log.Info("Preprocessing finished!");
        }
    }

    private ModelImport.ModelImporter InitializeModelImporter(string modelType, ModelConverter modelConverter, string rootDirectory)
    {
        switch (modelType)
        {
            case "UnityNative":
                return new GOModel(rootDirectory);
            case "ConversionRequired":
                    modelConverter.Convert(rootDirectory);
                return new ConvertedModel(modelConverter.OutputRootDir);
            case "ConvertedModel":
                return new ConvertedModel(rootDirectory);
            case "VolumetricModel":
                return new VolumetricModel(rootDirectory);
            default:
                throw Log.ThrowError("Incorrect ModelImporter type declared!", new IOException());
        }
    }
}
