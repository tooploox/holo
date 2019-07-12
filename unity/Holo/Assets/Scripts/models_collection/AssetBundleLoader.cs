using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class AssetBundleLoader
{
    private AssetBundle assetBundle;
    private string bundlePath;
    private List<ModelLayer> layers;

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
        LoadLayers();
    }

    private void LoadLayers()
    {
        layers = new List<ModelLayer>();
        foreach (string bundleObjectName in assetBundle.GetAllAssetNames())
        {
            if (!bundleObjectName.EndsWith(".prefab")) { continue; } // ignore other objects

            GameObject layerGameObject = assetBundle.LoadAsset<GameObject>(bundleObjectName);
            ModelLayer layer = layerGameObject.GetComponent<ModelLayer>();
            if (layer == null)
            {
                Debug.LogWarning("Prefab " + bundleObjectName + " does not contain ModelLayer component, guessing");
                layer = layerGameObject.AddComponent<ModelLayer>();
                layer.Caption = HoloUtilities.SuffixRemove(bundleObjectName, ".prefab");
                layer.DataFlow = bundleObjectName.Contains("dataflow");
            }

            layers.Add(layer);
        }
    }

    public IEnumerable<ModelLayer> Layers
    {
        get { return new ReadOnlyCollection<ModelLayer>(layers); }
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
            UnityEngine.Object.Instantiate(gameObject);
        }

    }
}
