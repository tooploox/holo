using System;
using System.IO;
using UnityEditor;
using UnityEngine;

using ModelImport;

public class DataPreparator
{
    /* Loads a model in batchmode or multiple models in Editor and converts them into an AssetBundle.
     * To use in batchmode: "<Path to Unity.exe>" -quit -batchmode -logFile "<Path to the logfile>"  
    * -executeMethod ModelLoader.LoadVTKModel -rootDirectory "<Directory of the folder which stores the meshes>" 
    */

    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    [MenuItem("Holo/Create AssetBundle from an external supported format")]
    public static void ImportWithConversion()
    {
        var importDispatcher = new DataPreparator();
        importDispatcher.PrepareData("ConversionRequired");
    }

    [MenuItem("Holo/Create AssetBundle from converted data")]
    public static void ImportConvertedModel()
    {
        var importDispatcher = new DataPreparator();
        importDispatcher.PrepareData("ConvertedModel");
    }

    [MenuItem("Holo/Create AssetBundle from a Unity-supported format")]
    public static void ImportGameObjectModel()
    {
        var importDispatcher = new DataPreparator();
        importDispatcher.PrepareData("UnityNative");
    }


    private void PrepareData(string modelType)
    {
        Log.Info("Preprocessing started!");
        var logConfig = new LoggingConfiguration();
        var assetBundleCreator = new AssetBundleCreator();
        var modelConverter = new ModelConverter();
        var modelImporter = InitializeModelImporter(modelType, modelConverter);
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
            Directory.Delete(modelConverter.TmpPath, true);
            if (AssetDatabase.IsValidFolder("Assets/tmp"))
            {
                FileUtil.DeleteFileOrDirectory("Assets/tmp");
                AssetDatabase.Refresh();
            }
        }
    }

    private ModelImport.ModelImporter InitializeModelImporter(string modelType, ModelConverter modelConverter)
    {
        string rootDirectory = GetRootDirectory();
        switch (modelType)
        {
            case "UnityNative":
                return new GOModel(rootDirectory);
            case "ConversionRequired":
                    modelConverter.Convert(rootDirectory);
                return new ConvertedModel(modelConverter.OutputRootDir);
            case "ConvertedModel":
                return new ConvertedModel(rootDirectory);
            default:
                var ex = new IOException();
                Log.Error("Incorrect Model Importer type declared!", ex);
                throw ex;
        }
    }

    private string GetRootDirectory()
    {
        string rootDirectory;

        if (Application.isBatchMode)
        {
            rootDirectory = GetBatchModeRootDir();
        }
        else
        {
            rootDirectory = EditorUtility.OpenFolderPanel("Select model root folder with ModelInfo.json", Application.dataPath, "");
        }
        if (string.IsNullOrEmpty(rootDirectory))
        {
            var ex = new IOException();
            Log.Error("Path cannot be null!", ex);
            throw ex;
        }
        return rootDirectory;
    }

    private string GetBatchModeRootDir()
    {
        string[] args = Environment.GetCommandLineArgs();
        int directoryFlagIndex = Array.FindIndex(args, a => a.Equals("-rootDirectory"));
        Log.Debug("rootDirectoryIndex:" + directoryFlagIndex.ToString());
        string rootDirectory = args[directoryFlagIndex + 1];
        if (string.IsNullOrEmpty(rootDirectory))
        {
            var ex = new IOException();
            Log.Error("Model's root directory has not been assigned!", ex);
            throw ex;
        }
        return rootDirectory;
    }
}
