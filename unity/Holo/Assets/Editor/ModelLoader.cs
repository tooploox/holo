using System;
using UnityEditor;
using UnityEngine;

class ModelLoader
{
    [MenuItem("Holo/Convert model to a AssetBundle's GameObject")]
    public static void PreprocessModel()
    {
        SingleModel importedModel = new SingleModel();
        AssetBundleCreator assetBundleCreator = new AssetBundleCreator();
        string rootDirectory = "";
        bool loadModel = true;

        while (loadModel)
        {
            rootDirectory = EditorUtility.OpenFolderPanel("Select model's root folder", Application.dataPath, "");
            if (String.IsNullOrEmpty(rootDirectory))
            {
                throw new ArgumentException("Path cannot be null!");
            }

            importedModel.GetModelData(rootDirectory);
            assetBundleCreator.Create(importedModel);
            loadModel = EditorUtility.DisplayDialog("", "Do you want to load another model?", "Yes", "No");
        }
    }
}
