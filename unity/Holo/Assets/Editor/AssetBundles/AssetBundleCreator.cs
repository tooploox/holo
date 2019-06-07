using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

class AssetBundleCreator
{
    public GameObject ModelGameObject { get; set; }
    public Mesh ModelMesh { get; set; }

    private string modelName;
    private Dictionary<string, string> assetsPath = new Dictionary<string, string>();
    private AssetBundleBuild[] buildMapArray;
    
    //Creates AssetBundle
    public void Create(SingleModel importedModel)
    {
        modelName = importedModel.ModelName;
        foreach (KeyValuePair<string, Tuple<Mesh, GameObject>> modelObject in importedModel.ModelObjects)
        {
            SaveFilesForExport(modelObject.Key, modelObject.Value);
        }
        BuildMapABs();
        SaveAssetBundles();
    }

    // Exports finished GameObject to a .prefab
    private void SaveFilesForExport(string objectName, Tuple<Mesh, GameObject> gameObjectData)
    {
        string rootAssetsDir = @"Assets/" + modelName;
        if (!AssetDatabase.IsValidFolder(rootAssetsDir))
        {
            AssetDatabase.CreateFolder("Assets", modelName);
        }
        assetsPath.Add("mesh", rootAssetsDir + @"/" + objectName + ".mesh");
        AssetDatabase.CreateAsset(gameObjectData.Item1, assetsPath["mesh"]);

        assetsPath.Add("GameObject", rootAssetsDir + @"/" + objectName + ".prefab");
        PrefabUtility.SaveAsPrefabAsset(gameObjectData.Item2, assetsPath["GameObject"]);
    }

    private void BuildMapABs()
    {
        // Create the array of bundle build details.
        AssetBundleBuild buildMap = new AssetBundleBuild();
        buildMap.assetBundleName = modelName + "_bundle";
        buildMap.assetNames = assetsPath.Values.ToArray();

        buildMapArray = new AssetBundleBuild[1] {buildMap};
    }

    private void SaveAssetBundles()
    {
        if (!AssetDatabase.IsValidFolder(@"Assets\StreamingAssets"))
        {
            AssetDatabase.CreateFolder("Assets", "StreamingAssets");
        }
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/StreamingAssets", buildMapArray, BuildAssetBundleOptions.None, BuildTarget.WSAPlayer);
        AssetDatabase.DeleteAsset("Assets/StreamingAssets/StreamingAssets");
        AssetDatabase.DeleteAsset("Assets/StreamingAssets/StreamingAssets.manifest");
    }
}
