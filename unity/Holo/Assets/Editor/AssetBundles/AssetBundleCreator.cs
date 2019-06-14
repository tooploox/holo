using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using ModelImport;

public class AssetBundleCreator
{
    private Dictionary<string, string> assetsPath = new Dictionary<string, string>();
    
    //Creates AssetBundle
    public void Create(SingleModel importedModel)
    {
        foreach (KeyValuePair<string, Tuple<Mesh, GameObject>> modelObject in importedModel.ModelObjects)
        {
            SaveFilesForExport(modelObject.Key, modelObject.Value, importedModel.ModelName);
        }
        AssetBundleBuild[] buildMapArray = BuildMapABs(importedModel.ModelName);
        CreateAssetBundle(buildMapArray);
    }

    // Exports finished GameObject to a .prefab
    private void SaveFilesForExport(string objectName, Tuple<Mesh, GameObject> gameObjectData, string modelName)
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
    
    // Create the array of bundle build details.
    private AssetBundleBuild[] BuildMapABs(string modelName)
    {
        
        AssetBundleBuild buildMap = new AssetBundleBuild();
        buildMap.assetBundleName = modelName + "_bundle";
        buildMap.assetNames = assetsPath.Values.ToArray();

        return new AssetBundleBuild[1] {buildMap};
    }
    //Creates appropriate AssetBundle for the model.
    private void CreateAssetBundle(AssetBundleBuild[] buildMapArray)
    {
        if (!AssetDatabase.IsValidFolder(@"Assets\StreamingAssets"))
        {
            AssetDatabase.CreateFolder("Assets", "StreamingAssets");
        }
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/StreamingAssets", buildMapArray, BuildAssetBundleOptions.None, BuildTarget.WSAPlayer);
        AssetDatabase.DeleteAsset("Assets/StreamingAssets/StreamingAssets");
        AssetDatabase.DeleteAsset("Assets/StreamingAssets/StreamingAssets.manifest");
        //TODO: The .mesh and .prefab files are left for debugging purposes but should be removed in the final version.
    }
}
