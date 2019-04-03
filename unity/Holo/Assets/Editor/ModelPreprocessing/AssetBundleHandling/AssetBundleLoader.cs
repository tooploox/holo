using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class AssetBundleLoader
{
    GameObject gameObject;
    Mesh mesh;

    [MenuItem("Holo/Load an Asset Bundle")]
    public static void Load()
    {
        AssetBundleLoader assetBundleLoader = new AssetBundleLoader();

        var assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "hypertrophy_bundle"));
        if (assetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }
        string[] assetPathArray = assetBundle.GetAllAssetNames();
        assetBundleLoader.LoadAssetFromBundle(assetBundle, assetPathArray);
        assetBundle.Unload(true);
    }

    private void LoadAssetFromBundle(AssetBundle assetBundle, string[] assetPathArray)
    {
        foreach (string assetPath in assetPathArray)
        {
            if (assetPath.EndsWith("prefab"))
            {
                AssetDatabase.CreateFolder("Assets", "Hypertrophy");
                gameObject = assetBundle.LoadAsset<GameObject>(assetPath);
                Object.Instantiate(gameObject);
            }
            if (assetPath.EndsWith("mesh"))
                mesh = assetBundle.LoadAsset<Mesh>(assetPath);
        }
        SkinnedMeshRenderer skinnedMesh = gameObject.GetComponent<SkinnedMeshRenderer>();
        skinnedMesh.sharedMesh = mesh;
    }
}
