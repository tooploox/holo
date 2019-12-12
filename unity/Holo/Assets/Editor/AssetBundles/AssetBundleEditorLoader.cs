using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleEditorLoader
{
    [MenuItem("Holo/Load all layers from an AssetBundle")]
    public static void LoadAssetBundle()
    {
        string bundlePath = EditorUtility.OpenFilePanel("Get The Bundle","","");
        if (string.IsNullOrEmpty(bundlePath)) {
            // "cancel" clicked
            return;
        }
        string bundleName = Path.GetFileName(bundlePath);
        AssetBundleLoader assetBundleLoader = new AssetBundleLoader(bundleName, bundlePath);
        assetBundleLoader.LoadBundle();
        assetBundleLoader.InstantiateAllLayers();
    }

    [MenuItem("Holo/Unload All Asset Bundles (allows to test again by 'Load all layers from an AssetBundle')")]
    public static void UnloadAllAssetBundles()
    {
        AssetBundle.UnloadAllAssetBundles(true);
    }
}
