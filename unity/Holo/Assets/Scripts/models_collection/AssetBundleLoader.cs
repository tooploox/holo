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

    public GameObject LoadGameObject(string sufix)
    {
        List<string> assetPathList = assetBundle.GetAllAssetNames().ToList();

        string endPattern = "_" + sufix + ".prefab";
        string gameObjectPath = assetPathList.Single(path => path.EndsWith(endPattern));
        gameObject = assetBundle.LoadAsset<GameObject>(gameObjectPath);
        return gameObject;
    }

    public GameObject LoadMainGameObject()
    {
        return LoadGameObject("body");
    }

    public GameObject InstantiateMainGameObject()
    {
        GameObject template = LoadMainGameObject();
        GameObject instance = UnityEngine.Object.Instantiate<GameObject>(template);
        return instance;
    }
}
