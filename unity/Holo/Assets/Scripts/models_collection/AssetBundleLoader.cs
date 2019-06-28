using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class AssetBundleLoader
{
    private AssetBundle assetBundle;
    private string bundlePath;

    private List<string> dataLayers;

    public void LoadBundle(string aBundlePath)
    {
        bundlePath = aBundlePath;
        var loadedAssetBundle = AssetBundle.LoadFromFile(bundlePath);
        if (loadedAssetBundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            return;
        }
        assetBundle = loadedAssetBundle;
        dataLayers = GetDataLayers();
    }

    private List<string> GetDataLayers()
    {
        var selectedNames = assetBundle.GetAllAssetNames()
                                .Where(name => name.EndsWith(".prefab"))
                                .Where(name => !name.EndsWith("_body.prefab"));
        
        return new List<string>(selectedNames);
    }

    public GameObject LoadGameObject(string sufix)
    {
        List<string> assetPathList = assetBundle.GetAllAssetNames().ToList();

        string endPattern = "_" + sufix + ".prefab";
        string gameObjectPath;
        try
        {
            gameObjectPath = assetPathList.Single(path => path.EndsWith(endPattern));
        } catch (InvalidOperationException e)
        {
            Debug.LogError("Probably no path with suffix " + endPattern + " in the asset bundle " + bundlePath + ", exception " + e.Message);
            // Since the caller probably doesn't except null result, above makes LogError instead of LogWarning
            return null;
        }

        return assetBundle.LoadAsset<GameObject>(gameObjectPath);
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
