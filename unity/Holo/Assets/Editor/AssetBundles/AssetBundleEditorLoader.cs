using UnityEngine;
using UnityEditor;

public class AssetBundleEditorLoader
{
    [MenuItem("Holo/Load all layers from an AssetBundle")]
    public static void LoadAssetBundle()
    {
        AssetBundleLoader assetBundleLoader = new AssetBundleLoader();
        string bundlePath = EditorUtility.OpenFilePanel("Get The Bundle","","");
        assetBundleLoader.LoadBundle(bundlePath);
        assetBundleLoader.InstantiateAllLayers();
    }

    [MenuItem("Holo/Unload All Asset Bundles (allows to test again by 'Load all layers from an AssetBundle')")]
    public static void UnloadAllAssetBundles()
    {
        AssetBundle.UnloadAllAssetBundles(true);
    }
}
