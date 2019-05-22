using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

class AssetBundleLoader
{
    private AssetBundle assetBundle;
    private string bundlePath;

    private GameObject gameObject;
    private Mesh mesh;

    [MenuItem("Holo/Load an Asset Bundle")]
    public static void Load()
    {
        AssetBundleLoader assetBundleLoader = new AssetBundleLoader();

        string bundlePath = EditorUtility.OpenFilePanel("Choose AssetBundle to be loaded.", "", "");
        assetBundleLoader.LoadBundle(bundlePath);
        assetBundleLoader.LoadAssetFromBundle();
    }

    private void LoadBundle(string bundlePath)
    {
        var loadedAssetBundle = AssetBundle.LoadFromFile(bundlePath);
        if (loadedAssetBundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            return;
        }
        assetBundle = loadedAssetBundle;
    }

    private void LoadAssetFromBundle()
    {
        List<string> assetPathList = assetBundle.GetAllAssetNames().ToList();

        string gameObjectPath = assetPathList.Single(path => path.EndsWith(".prefab"));
        string meshPath = assetPathList.Single(path => path.EndsWith(".mesh"));

        gameObject = assetBundle.LoadAsset<GameObject>(gameObjectPath);
        mesh = assetBundle.LoadAsset<Mesh>(meshPath);

        SkinnedMeshRenderer skinnedMesh = gameObject.GetComponent<SkinnedMeshRenderer>();
        skinnedMesh.sharedMesh = mesh;
        Object.Instantiate(gameObject);
        Mesh.Instantiate(mesh);
    }
}
