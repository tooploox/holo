using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

class AssetBundleCreator
{
    public GameObject ModelGameObject { get; set; }
    public Mesh Mesh { get; set; }

    private string rootAssetsDir;
    private List<string> assetsPath = new List<string>();
    AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
    
    //Creates AssetBundle
    public void Create()
    {
        rootAssetsDir = @"Assets/" + ModelGameObject.name;
        SaveFilesForExport();
        BuildMapABs();
        AssetDatabase.CreateFolder("Assets", ModelGameObject.name + "_bundles");
        BuildPipeline.BuildAssetBundles(rootAssetsDir + "_bundles", buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    // Exports finished GameObject to a .prefab
    private void SaveFilesForExport()
    {
        AssetDatabase.CreateFolder("Assets", ModelGameObject.name);

        assetsPath.Add(rootAssetsDir + @"/" + ModelGameObject.name + ".mesh");
        AssetDatabase.CreateAsset(Mesh, assetsPath[0]);

        assetsPath.Add(rootAssetsDir + @"/" + ModelGameObject.name + ".prefab");
        PrefabUtility.SaveAsPrefabAsset(ModelGameObject, assetsPath[1]);       
    }

    private void BuildMapABs()
    {
        // Create the array of bundle build details.
        buildMap[0].assetBundleName = ModelGameObject.name + "_bundle";
        buildMap[0].assetNames = assetsPath.ToArray();
    }
}
