using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class ModelPreprocessor
{
    private GameObject modelGameObject;
    private Mesh mesh;
    private AssetBundleCreator assetBundleCreator = new AssetBundleCreator();
    private string rootDirectory;
    private List<string> assetFilePaths = new List<string>();

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
        rootDirectory = EditorUtility.OpenFolderPanel("Select STL series root folder", Application.dataPath, "");
        if (String.IsNullOrEmpty(rootDirectory))
            throw new ArgumentException("Path cannot be null!");
    }

    private void GetModelData()
    {
        FileSeriesImporter seriesImporter = new FileSeriesImporter(rootDirectory);
        modelGameObject = seriesImporter.ModelGameObject;
        mesh = seriesImporter.Mesh;
    }


}
