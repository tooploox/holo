using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AssetBundleLoader
{
    private AssetBundle assetBundle;
    private string bundlePath;

    private GameObject gameObject;
    private Mesh mesh;

    public void LoadBundle(string bundlePath)
    {
        var loadedAssetBundle = AssetBundle.LoadFromFile(bundlePath);
        if (loadedAssetBundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            return;
        }
        assetBundle = loadedAssetBundle;
    }

    public void LoadAssetFromBundle()
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

        /* TODO: testing 
        GameObject goInstance = Object.Instantiate<GameObject>(gameObject);
        Mesh meshInstance = Object.Instantiate<Mesh>(mesh);
        SkinnedMeshRenderer skinnedMesh = goInstance.GetComponent<SkinnedMeshRenderer>();
        skinnedMesh.sharedMesh = meshInstance;
        */
    }
}
