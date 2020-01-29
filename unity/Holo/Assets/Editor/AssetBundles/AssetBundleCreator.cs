using System.Linq;
using UnityEditor;
using UnityEngine;

using ModelImport;

public class AssetBundleCreator
{  
    //Creates AssetBundle
    public void Create(ModelImport.ModelImporter importedModel)
    {
        AssetBundleBuild[] buildMapArray = BuildMapABs(importedModel);
        CreateAssetBundle(buildMapArray);
    }

    // Create the array of bundle build details.
    private AssetBundleBuild[] BuildMapABs(ModelImport.ModelImporter importedModel)
    {
        
        AssetBundleBuild buildMap = new AssetBundleBuild();
        buildMap.assetBundleName = importedModel.Info.Caption + "_bundle";
        buildMap.assetNames = importedModel.AssetsPath.Values.ToArray();

        return new AssetBundleBuild[1] {buildMap};
    }
    //Creates appropriate AssetBundle for the model.
    private void CreateAssetBundle(AssetBundleBuild[] buildMapArray)
    {
        if (!AssetDatabase.IsValidFolder(@"Assets\StreamingAssets"))
        {
            AssetDatabase.CreateFolder("Assets", "StreamingAssets");
        }
        AssetDatabase.Refresh();
        BuildPipeline.BuildAssetBundles(Application.dataPath + "/StreamingAssets", buildMapArray, BuildAssetBundleOptions.None, BuildTarget.WSAPlayer);
        AssetDatabase.DeleteAsset("Assets/StreamingAssets/StreamingAssets");
        AssetDatabase.DeleteAsset("Assets/StreamingAssets/StreamingAssets.manifest");
        //TODO: The .mesh and .prefab files are left for debugging purposes but should be removed in the final version.
    }
}
