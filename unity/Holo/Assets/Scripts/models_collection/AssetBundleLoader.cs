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
                layer.Caption = HoloUtilities.SuffixRemove(".prefab", bundleObjectName);
                layer.DataFlow = bundleObjectName.Contains("dataflow");
            }

            layers.Add(layer);
        }
    }

    public IEnumerable<ModelLayer> Layers
    {
        get { return new ReadOnlyCollection<ModelLayer>(layers); }
    }

    public void InstantiateAllLayers()
    {
        foreach (ModelLayer layer in Layers)
        {
            layer.InstantiateGameObject(null);
        }
        return modelGameObjects;
    }
}
