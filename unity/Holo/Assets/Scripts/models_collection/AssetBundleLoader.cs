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

    public GameObject LoadMainGameObject()
    {
        List<string> assetPathList = assetBundle.GetAllAssetNames().ToList();

        string gameObjectPath = assetPathList.Single(path => path.EndsWith(".prefab"));
        gameObject = assetBundle.LoadAsset<GameObject>(gameObjectPath);
        return gameObject;
    }

    public GameObject InstantiateMainGameObject()
    {
        GameObject template = LoadMainGameObject();
        GameObject instance = UnityEngine.Object.Instantiate<GameObject>(template);
        return instance;
    }

    public List<GameObject> LoadMultipleGameObjects()
    {
        List<string> assetPathList = assetBundle.GetAllAssetNames().ToList();
        List<GameObject> modelGameObjects = new List<GameObject>();
        List<string> gameObjectPaths = assetPathList.FindAll(path => path.EndsWith(".prefab"));
        foreach (string path in gameObjectPaths)
        {
            modelGameObjects.Add(assetBundle.LoadAsset<GameObject>(path));
        }
        return modelGameObjects;
    }

    public void InstantiateMultipleGameObjects()
    {
        List<GameObject> template = LoadMultipleGameObjects();

        foreach (GameObject gameObject in template)
        {
            Object.Instantiate(gameObject);
        }

    }
}
