using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class ModelPreprocessor
{
    GameObject modelGameObject;
    Mesh mesh;
    AssetBundleCreator assetBundleCreator = new AssetBundleCreator();
    string rootFolder;
    List<string> assetFilePaths = new List<string>();

    [MenuItem("Holo/Convert model to a AssetBundle's GameObject")]
    public static void PreprocessModel()
    {
        ModelPreprocessor modelPreprocessor = new ModelPreprocessor();

        modelPreprocessor.GetRootFolder();

        modelPreprocessor.GetModelData();

        modelPreprocessor.SaveModelAsAssetBundle();
    }

    // gets path to the root folder.
    private void GetRootFolder()
    {
        rootFolder = EditorUtility.OpenFolderPanel("Select STL series root folder", Application.dataPath, "");
        if (String.IsNullOrEmpty(rootFolder))
            throw new ArgumentException("Path cannot be null!");
    }

    private void GetModelData()
    {
        FileSeriesImporter seriesImporter = new FileSeriesImporter(rootFolder);
        modelGameObject = seriesImporter.GetGameObject();
        mesh = seriesImporter.GetMesh();
    }


    private void SaveModelAsAssetBundle()
    {
        assetBundleCreator.Mesh = mesh;
        assetBundleCreator.ModelGameObject = modelGameObject;
        assetBundleCreator.Create();
    }


}
