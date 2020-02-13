using System.Collections.Generic;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

using ModelImport;

public class DataPreparator
{
    /* Loads a model in batchmode or multiple models in Editor and converts them into an AssetBundle.
     * To use in batchmode: "<Path to Unity.exe>" -quit -batchmode -logFile "<Path to the logfile>"  
    * -executeMethod ModelLoader.ImportWithConversion() -rootDirectory "<Directory of the folder which stores the meshes>" 
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
        var inputInfo = new InputInfo();
        var assetBundleCreator = new AssetBundleCreator(inputInfo.OutputDir);
        var modelConverter = new ModelConverter();

        Log.Info("Preprocessing started!");
        var modelImporter = InitializeModelImporter(modelType, modelConverter, inputInfo.RootDirectory);
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
            default:
                var ex = new IOException();
                Log.Error("Incorrect Model Importer type declared!", ex);
                throw ex;
        }
    }
}
