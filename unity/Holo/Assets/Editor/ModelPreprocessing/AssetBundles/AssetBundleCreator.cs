using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

class AssetBundleCreator
{
    public GameObject ModelGameObject { get; set; }
    public Mesh Mesh { get; set; }

    private string rootAssetsDir;
    private Dictionary<string, string> assetsPath = new Dictionary<string, string>();
    private AssetBundleBuild[] buildMapArray;
    
    //Creates AssetBundle
    public void Create(GameObject modelGameObject, Mesh mesh)
    {
        Mesh = mesh;
        ModelGameObject = modelGameObject;

        rootAssetsDir = @"Assets/" + ModelGameObject.name;
        SaveFilesForExport();
        BuildMapABs();
        SaveAssetBundles();
    }

    // Exports finished GameObject to a .prefab
    private void SaveFilesForExport()
    {
        if (!AssetDatabase.IsValidFolder(@"Assets\" + ModelGameObject.name))
        {
            AssetDatabase.CreateFolder("Assets", ModelGameObject.name);
        }
        assetsPath.Add("mesh", rootAssetsDir + @"/" + ModelGameObject.name + ".mesh");
        AssetDatabase.CreateAsset(Mesh, assetsPath["mesh"]);

        assetsPath.Add("GameObject", rootAssetsDir + @"/" + ModelGameObject.name + ".prefab");
        PrefabUtility.SaveAsPrefabAsset(ModelGameObject, assetsPath["GameObject"]);       
    }

    private void BuildMapABs()
    {
        // Create the array of bundle build details.
        AssetBundleBuild buildMap = new AssetBundleBuild();
        buildMap.assetBundleName = ModelGameObject.name + "_bundle";
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
